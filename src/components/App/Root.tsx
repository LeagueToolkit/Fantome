import React from "react"
import { Provider } from "react-redux"
import { store } from "../../redux/store"
import { App } from "./app"

export const Root: React.FC<{}> = () => {
    return (
        <Provider store={store}>
            <App></App>
        </Provider>
    )
}
