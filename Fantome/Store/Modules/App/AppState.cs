using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Store.Modules.App
{
    public record AppState
    {
        public Dictionary<string, ActionState> ActionStates { get; init; }

        public ActionState GetActionState(Type actionType)
        {
            if (this.ActionStates.ContainsKey(actionType.Name))
            {
                return this.ActionStates[actionType.Name];
            }
            else
            {
                return new() { Status = ActionStatus.Idle };
            }
        }
    }

    public record ActionState
    {
        public ActionStatus Status { get; init; }
        public Exception Error { get; init; }
    }

    public enum ActionStatus
    {
        Idle,
        Processing,
        Finished,
        Failure
    }
}
