using UnityEngine;

using System;

public abstract class CustomBuildSetupEnv
{
    AppcoinsGameObject appcoinsGameObject;

    public CustomBuildSetupEnv(AppcoinsGameObject a)
    {
        appcoinsGameObject = a;
    }

    public virtual void Setup()
    {
        try
        {
            appcoinsGameObject.CheckAppcoinsGameobject();
        }

        catch (Exception e)
        {
            throw e;
        }
    }
}