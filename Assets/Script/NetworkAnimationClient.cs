using UnityEngine;
using Unity.Netcode.Components;

public class NetworkAnimationClient : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
