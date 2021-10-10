import { ActionType, createAsyncAction } from "typesafe-actions"
import { ConfigState } from "./types"

export const configActions = {
    fetch: createAsyncAction("@config/FETCH_REQUEST", "@config/FETCH_SUCCESS", "@config/FETCH_ERROR")<
        void,
        ConfigState,
        void
    >(),
}

export type ConfigAction = ActionType<typeof configActions>
