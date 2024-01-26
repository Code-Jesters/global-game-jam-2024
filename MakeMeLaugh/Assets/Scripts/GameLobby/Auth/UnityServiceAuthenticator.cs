using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

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
            if (UnityServices.State == ServicesInitializationState.Initialized)
                return true;

            //Another Service is mid-initialization:
            if (UnityServices.State == ServicesInitializationState.Initializing)
            {
                var task = WaitForInitialized();
                if (await Task.WhenAny(task, Task.Delay(k_InitTimeout)) != task)
                    return false; // We timed out

                return UnityServices.State == ServicesInitializationState.Initialized;
            }

            if (profileName != null)
            {
                //ProfileNames can't contain non-alphanumeric characters
                Regex rgx = new Regex("[^a-zA-Z0-9 - _]");
                profileName = rgx.Replace(profileName, "");
                var authProfile = new InitializationOptions().SetProfile(profileName);

                //If you are using multiple unity services, make sure to initialize it only once before using your services.
                await UnityServices.InitializeAsync(authProfile);
            }
            else
                await UnityServices.InitializeAsync();

            return UnityServices.State == ServicesInitializationState.Initialized;

            async Task WaitForInitialized()
            {
                while (UnityServices.State != ServicesInitializationState.Initialized)
                    await Task.Delay(100);
            }
        }

        public static async Task<bool> TrySignInAsync(string profileName = null)
        {
            if (!await TryInitServicesAsync(profileName))
                return false;
            if (s_IsSigningIn)
            {
                var task = WaitForSignedIn();
                if (await Task.WhenAny(task, Task.Delay(k_InitTimeout)) != task)
                    return false; // We timed out
                return AuthenticationService.Instance.IsSignedIn;
            }

            s_IsSigningIn = true;
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            s_IsSigningIn = false;

            return AuthenticationService.Instance.IsSignedIn;

            async Task WaitForSignedIn()
            {
                while (!AuthenticationService.Instance.IsSignedIn)
                    await Task.Delay(100);
            }
        }
    }
}
