import { ConnectedRouter } from "connected-react-router"
import React, { useState } from "react"
import { Route } from "react-router"
import { Switch } from "react-router-dom"
import { CustomProvider } from "rsuite"
import "rsuite/dist/rsuite.min.css"
import { history } from "../../redux/store"
import { AppRoute } from "../../routes"
import { Layout } from "../Layout"
import "./app.css"
import { GlobalStyle } from "./GlobalStyle"

interface CustomResponse {
    message: string
}

export type AppProps = {}

export const App: React.FC<AppProps> = () => {
    const [rustMsg, setRustMsg] = useState("")

    return (
        <ConnectedRouter history={history}>
            <CustomProvider theme="dark">
                <GlobalStyle />
                <Layout>
                    <Switch>
                        <Route exact path={AppRoute.HOME}></Route>
                    </Switch>
                </Layout>
            </CustomProvider>
        </ConnectedRouter>
    )
}
