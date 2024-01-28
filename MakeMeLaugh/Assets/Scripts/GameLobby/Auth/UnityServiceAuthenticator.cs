using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
//using Unity.Services.Core.Environments;
using System;
using Cysharp.Threading.Tasks;

using UnityEngine;
using Unity.Netcode; // for Debug.Log()

namespace Unity.Services.Samples
{
    /// <summary>
    /// Sample implementation of the Unity Authentication Service for Anonymous Auth
    /// Handles Race conditions between different sources of authentication, allowing multiple samples to be dragged into a scene without errors.
    /// (In a real project, you should ensure a single-entry point for authentication.)
    /// </summary>
    public static class UnityServiceAuthenticator
    {
        const int k_InitTimeout = 10000;
        static bool s_IsSigningIn;

        /// <summary>
        /// Unity anonymous Auth grants unique ID's by editor/build and machine. This means that if you open several builds or editors on the same machine, they will all have the same ID.
        /// Using a unique profile name forces a new ID. So the strategy is to make sure that each build/editor has its own profile name to act as multiple users for a service.
        /// </summary>
        /// <param name="profileName">Unique name that generates the unique ID</param>
        /// <returns></returns>
        public static async Task<bool> TryInitServicesAsync(string profileName = null)
        {
            Debug.Log("TryInitServicesAsync() start");
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                Debug.Log("TryInitServicesAsync() already true");
                return true;
            }

            //Another Service is mid-initialization:
            if (UnityServices.State == ServicesInitializationState.Initializing)
            {
                var task = WaitForInitialized();
                var checkTask = await Task.WhenAny(task, Task.Delay(k_InitTimeout));
                if (checkTask != task)
                {
                    Debug.Log("TryInitServicesAsync() timed out");
                    return false; // We timed out
                }

                Debug.Log("TryInitServicesAsync() concluding " + (UnityServices.State == ServicesInitializationState.Initialized));
                return UnityServices.State == ServicesInitializationState.Initialized;
            }

            if (profileName != null)
            {
                //ProfileNames can't contain non-alphanumeric characters
                Regex rgx = new Regex("[^a-zA-Z0-9 - _]");
                profileName = rgx.Replace(profileName, "");
                var authProfile = new InitializationOptions()
                    .SetProfile(profileName)
                    // Does not compile for me (but I guess "production" is default anyhow?)
                    // .SetEnvironmentName("production")
                    ;

                //
                        //var options = new InitializationOptions().SetEnvironmentName("production");

                //

                //If you are using multiple unity services, make sure to initialize it only once before using your services.
                Debug.Log("TryInitServicesAsync() calling UnityServices.InitializeAsync() w/ auth profile");
                try
                {
                    await UnityServices.InitializeAsync(authProfile);
                }
                catch (Exception exception)
                {
                    Debug.Log("UnityServices.InitializeAsync() w/ auth profile failed w/ exception " + exception.ToString());
                }
                Debug.Log("UnityServices.InitializeAsync() w/ auth profile has finished");
            }
            else
            {
                Debug.Log("TryInitServicesAsync() calling UnityServices.InitializeAsync() w/out auth profile");
                await UnityServices.InitializeAsync();
            }

            Debug.Log("TryInitServicesAsync() reached end; concluding " + (UnityServices.State == ServicesInitializationState.Initialized));
            return UnityServices.State == ServicesInitializationState.Initialized;

            async Task WaitForInitialized()
            {
                Debug.Log("WaitForInitialized() start");
                while (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UniTask.Delay(100);
                }
                Debug.Log("WaitForInitialized() reached end");
            }
        }

        public static async Task<bool> TrySignInAsync(string profileName = null)
        {
            Debug.Log("TrySignInAsync() start");
            if (!await TryInitServicesAsync(profileName))
            {
                Debug.Log("TrySignInAsync() return false (1)");
                return false;
            }
            if (s_IsSigningIn)
            {
                var task = WaitForSignedIn();
                if (await Task.WhenAny(task, Task.Delay(k_InitTimeout)) != task)
                {
                    Debug.Log("TrySignInAsync() return false (2)");
                    return false; // We timed out
                }
                Debug.Log("TrySignInAsync() returning " + AuthenticationService.Instance.IsSignedIn);
                return AuthenticationService.Instance.IsSignedIn;
            }

            s_IsSigningIn = true;
            Debug.Log("TrySignInAsync() awaiting AuthenticationService.Instance.SignInAnonymouslyAsync()");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("TrySignInAsync() finished awaiting AuthenticationService.Instance.SignInAnonymouslyAsync()");
            s_IsSigningIn = false;

            Debug.Log("TrySignInAsync() returning " + AuthenticationService.Instance.IsSignedIn);
            return AuthenticationService.Instance.IsSignedIn;

            async Task WaitForSignedIn()
            {
                while (!AuthenticationService.Instance.IsSignedIn)
                {
                    await Task.Delay(100);
                }
            }
        }
    }
}
