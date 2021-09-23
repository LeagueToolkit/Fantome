using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Store.Modules.Config;
using Fluxor;
using Serilog;

namespace Fantome.Store.Modules.App
{
    public class Reducers
    {
        [ReducerMethod]
        public static AppState HandleAsyncActionRequest(AppState state, AsyncActionRequest action)
        {
            Type asyncActionDeclaringType = action.GetType().DeclaringType;

            Dictionary<string, ActionState> actionStates = state.ActionStates;

            if (actionStates.TryAdd(asyncActionDeclaringType.Name, new() { Status = ActionStatus.Processing }) is false)
            {
                actionStates[asyncActionDeclaringType.Name] = new() { Status = ActionStatus.Processing };
            }

            return state with { ActionStates = actionStates };
        }

        [ReducerMethod]
        public static AppState HandleAsyncActionSuccess(AppState state, AsyncActionSuccess action)
        {
            Type asyncActionDeclaringType = action.GetType().DeclaringType;

            Dictionary<string, ActionState> actionStates = state.ActionStates;

            if (actionStates.TryAdd(asyncActionDeclaringType.Name, new() { Status = ActionStatus.Finished }) is false)
            {
                actionStates[asyncActionDeclaringType.Name] = new() { Status = ActionStatus.Finished };
            }

            return state with { ActionStates = actionStates };
        }

        [ReducerMethod]
        public static AppState HandleAsyncActionFailure(AppState state, AsyncActionFailure action)
        {
            Type asyncActionDeclaringType = action.GetType().DeclaringType;

            Dictionary<string, ActionState> actionStates = state.ActionStates;
            ActionState actionState = new() { Status = ActionStatus.Failure, Error = action.Error };

            if (actionStates.TryAdd(asyncActionDeclaringType.Name, actionState) is false)
            {
                actionStates[asyncActionDeclaringType.Name] = actionState;
            }

            return state with { ActionStates = actionStates };
        }
    }
}
