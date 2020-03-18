module Server

open System
open Giraffe
open Saturn
open Config
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System.Security.Claims
open Serilog
open Serilog.Sinks
open Serilog.Events
open Microsoft.WindowsAzure.Storage
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Serilog
open System.IO
open Microsoft.AspNetCore.Authentication.Cookies
open System.Threading.Tasks
open CAS

let setupConfiguration (config:IConfiguration) =
    {
        connectionString = config.["ConnectionStrings:DefaultConnection"]
        edsUrl = config.["EDS:Url"]
        webAuthUrl = config.["WebAuth:Url"]
        edsUserName = config.["EDS:UserName"]
        edsPassword = config.["EDS:Password"]
        configSettingExample = config.["EDS:ConfigSettingExample"]
        environment = config.["Environment"]
        blobStorageConnectionString = config.["Azure:BlobStorageConnectionString"]
        sink = config.["Logging:Sink"]
    }

//https://github.com/chriswill/serilog-sinks-azureblobstorage

let configSerilog (builder:ILoggingBuilder) =
    builder.SetMinimumLevel(LogLevel.Information) |> ignore
    let config = builder.Services.BuildServiceProvider().GetService<IConfiguration>()
    let loggingConfig = new LoggerConfiguration()
    loggingConfig.Enrich.FromLogContext() |> ignore
    match config.["Logging:Sink"] with
    //| "File" -> loggingConfig.WriteTo.File("D:\\log.log", rollingInterval = RollingInterval.Day) |> ignore
    | "Console" -> loggingConfig.WriteTo.Console() |> ignore
    | _ -> loggingConfig.WriteTo.Console() |> ignore
    builder.AddSerilog(loggingConfig.CreateLogger()) |> ignore

let setupCookies (options:CookieAuthenticationOptions) =
    let cookieEvents = new CookieAuthenticationEvents()
    cookieEvents.OnSigningIn <- (fun ctx ->
        
            //TODO uncomment this code and make work for your application
            //let getClaims (attributes: UserAttribute list) =
            //    let identity = new ClaimsIdentity()
            //    for attr in attributes do 
            //        for value in attr.Values do
            //            if attr.Name = "isMemberOf" then
            //                if value.Contains("dept:COM:medsrp") then //ONLY get the groups in MedSRP
            //                    identity.AddClaim(new Claim(ClaimTypes.Role, Authorization.getRole value))
            //            else
            //                identity.AddClaim(new Claim(attr.Name, value))              
            //    identity

            let cnf = ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>()
            //TODO uncomment and make work for your application
            //let edsService = new DirectoryServices.EdsClient(cnf.["EDS:Url"], cnf.["EDS:UserName"], cnf.["EDS:Password"]) :> Interfaces.IEdsClient
            //let userInfo = edsService.GetUserInfo(ctx.Principal.Identity.Name)
            //ctx.Principal.AddIdentity(getClaims(userInfo.Attributes))
            Task.CompletedTask)
        
    options.Events <- cookieEvents

    options.Cookie.SameSite <- SameSiteMode.Strict
    options.Cookie.HttpOnly <- true
    options.Cookie.SecurePolicy <- CookieSecurePolicy.Always
    options.ExpireTimeSpan <- TimeSpan.FromMinutes(30.0)
    options.SlidingExpiration <- true

let configureServices (services : IServiceCollection) =
    services.AddSession(fun opts ->
        opts.IdleTimeout <- TimeSpan.FromMinutes(30.0)
    ) |> ignore
    
    services
    
let configureApp (app:IApplicationBuilder) =
    app.UseHsts() |> ignore
    app
    
let endpointPipe = pipeline {
    plug head
    plug requestId
}
let app = application {
    app_config configureApp
    pipe_through endpointPipe
    logging configSerilog
    error_handler (fun ex logger -> 
                                    logger.LogCritical(ex.Message)
                                    pipeline { render_html (InternalError.layout ex) }
                                    )
    use_router Router.appRouter
    url "https://saturn.local:443/"
    memory_cache
    use_static "static"
    use_gzip
    use_config setupConfiguration 
    use_iis
    use_cas "webauth url"
    use_cookies_authentication_with_config (fun options -> setupCookies options)
    force_ssl
}

[<EntryPoint>]
let main _ =
    printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())
    run app
    0 // return an integer exit code