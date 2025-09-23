// this disables the card back mseh blocker 
// for now we're using texturereplacer to disable this instead
// so my code can stay simple as I do R&D
//     Find card back mesh renderer and disable it
//     Transform cardBackBlocker = __instance.m_GradedCardGrp?.transform.Find("CardBackMeshBlocker");
//     Renderer cardBackRenderer = cardBackBlocker?.GetComponent<Renderer>();

//     if (cardBackRenderer != null)
//     {
//         // cardBackRenderer.enabled = false;
//         Logger.LogInfo($"CardBackMeshBlocker disabled for grade {grade}");
//     }    