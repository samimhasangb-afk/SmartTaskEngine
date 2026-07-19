namespace SmartTaskEngine.Models.Enums
{
    // Named "TaskState" on purpose - System.Threading.Tasks already defines a
    // "TaskStatus" enum, and every async controller/service file needs
    // "using System.Threading.Tasks;", so re-using that name would cause
    // ambiguous-reference compile errors.
    public enum TaskState
    {
        ToDo = 0,
        InProgress = 1,
        Done = 2
    }
}
