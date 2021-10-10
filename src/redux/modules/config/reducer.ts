import { Reducer } from "redux"
import { getType } from "typesafe-actions"
import { configActions } from "."
import { ConfigAction } from "./actions"
import { ConfigState } from "./types"

const initialState: ConfigState = {
    gameLocation: "",
}

export const configReducer: Reducer<ConfigState, ConfigAction> = (state = initialState, action) => {
    switch (action.type) {
        case getType(configActions.fetch.success): {
            return { ...action.payload }
        }
        default:
            return state
    }
}
