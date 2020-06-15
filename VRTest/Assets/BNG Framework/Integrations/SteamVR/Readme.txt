SteamVR Support is current experimental. If you want to give SteamVR a go, this integration package can get you started.

1. First install the SteamVR asset from the asset store : https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647

2. Extract the included SteamVR integration package (SteamVR.unityPackage)

3. Click "Yes" to import the provided Input Actions / Bindings into SteamVR. You can integrate them with your own bindings, or overwrite them completely.

4. Add "STEAM_VR_SDK" (no quotes) to your Scripting Define Symbols (Edit -> Project Settings -> Other -> Scripting Define Symbols)

5. On Player's InputBridge object, make sure "SteamVR" is selected as the input source.

Note : This essentially maps SteamVR Actions such as "Grip", "Trigger", etc. so that the InputBridge can convert it to be used as raw input. It's not how SteamVR's input system is intended to be used, but is currently the only way to get input from certain devices until Unity gets full OpenVR support.

- Check out InputBridge.cs to see how Steam Actions are bound to inputs