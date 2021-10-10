import { connectRouter } from "connected-react-router"
import { History } from "history"
import { combineReducers } from "redux"

export const createRootReducer = (history: History) =>
    combineReducers({
        router: connectRouter(history),
    })
