using UnityEngine.Events;

public enum BuildStage
{
    SETUP_ENV,
    IDLE,
    UNITY_EXPORT,
    PROJECT_BUILD,
    PROJECT_INSTALL,
    PROJECT_RUN,
    DONE,
}

public class BuildStageEvent : UnityEvent<BuildStage> { }
