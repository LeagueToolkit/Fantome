import createSagaMiddleware from "@redux-saga/core"
import { routerMiddleware } from "connected-react-router"
import { createBrowserHistory } from "history"
import { applyMiddleware, createStore } from "redux"
import { composeWithDevTools } from "redux-devtools-extension"
import { createRootReducer } from "./rootReducer"
import { rootSaga } from "./rootSaga"

const sagaMiddleware = createSagaMiddleware()

export const history = createBrowserHistory()

export const store = createStore(
    createRootReducer(history),
    composeWithDevTools(applyMiddleware(sagaMiddleware, routerMiddleware(history))),
)

sagaMiddleware.run(rootSaga)
