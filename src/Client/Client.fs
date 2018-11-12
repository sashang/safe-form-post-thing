module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core.JsInterop
open Fable.PowerPack
open Fable.PowerPack.Fetch

open Thoth.Json

open Shared

open Fulma


// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = { username : string }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
    | ClickLogin
    | LoginSuccess of Login
    | AuthError of exn
    | SetUsername of string 


// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let initialModel = { username = "" }
    initialModel, Cmd.none

let authUser model =
    promise {

        let body = Encode.Auto.toString(0, model)

        let props =
            [ Fetch.requestHeaders [
                  HttpRequestHeaders.ContentType "application/json" ]
              RequestProperties.Body !^body ]

        try
            let decoder = Decode.Auto.generateDecoder<Login>()
            let! res = Fetch.fetchAs<Login> "/api/login" decoder props
            return res
        with _ ->
            return! failwithf "Could not authenticate user."
    }

let authUserCmd login =
    Cmd.ofPromise authUser login LoginSuccess AuthError

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    match msg with
    | LoginSuccess username ->
        model, Cmd.none
    | ClickLogin ->
        model, authUserCmd model
    | SetUsername username ->
        {model with username = username}, Cmd.none
    | AuthError err ->
        failwithf("failed to lgin")

let view (model : Model) (dispatch : Msg -> unit) =
    div [] [
        Navbar.navbar [ Navbar.Color IsPrimary ] [
            Navbar.Item.div [ ] [
                Heading.h2 [ ] [
                    str "SAFE Template"
                ]
            ]
        ] 

        Container.container [] [
            Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                [ Heading.h3 [] [ str ("Login form") ] ]
            div [ ClassName "input-group input-group-lg" ] [
                span [ClassName "input-group-addon" ] [
                    span [ClassName "glyphicon glyphicon-asterisk"] []
                ]
                form [] [
                    input [
                        Id "username"
                        Key ("username" + model.username)
                        HTMLAttr.Type "username"
                        ClassName "form-control input-lg"
                        Placeholder "usernaem"
                        DefaultValue model.username
                        OnChange (fun ev -> dispatch (SetUsername ev.Value)) ] 
                    Button.button [
                          Button.Color IsPrimary
                          Button.IsFullWidth
                          Button.OnClick (fun _ -> (dispatch ClickLogin))
                          Button.CustomClass "is-large is-block" ] [ str "Login" ] 
               ]
            ]
        ]
    ]



#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
