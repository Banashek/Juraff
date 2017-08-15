module HelloWeb.App

open System
open System.IO
open System.Collections.Generic
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Juraff.HttpHandlers
open Juraff.Middleware
open Juraff.XmlViewEngine


/// Views


let layout (content: XmlNode list) =
    html [] [
        head [] [
            title []  [ encodedText "Giraffe" ]
        ]
        body [] content
    ]

let partial () =
    p [] [ encodedText "Some partial text." ]

let mainView =
  [ div [] [ partial() ]] |> layout


/// App


let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> (mainView |> renderHtml)
            ]
        setStatusCode 404 >=> text "Not Found" ]


/// Config

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffeErrorHandler errorHandler
    app.UseGiraffe webApp

let configureServices (services : IServiceCollection) = ()       

let configureLogging (loggerBuilder : ILoggingBuilder) =
    loggerBuilder.AddConsole().AddFilter("System", LogLevel.Debug) |> ignore

[<EntryPoint>]
let main argv =
    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(Action<IServiceCollection> configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0