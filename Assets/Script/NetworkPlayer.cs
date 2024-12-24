using UnityEngine;
using Unity.Netcode;
using System.Globalization;

public class NetworkPlayer : NetworkBehaviour
{
    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public Renderer[] meshToDisable; // Các mesh sẽ bị tắt khi là Owner

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Tắt mesh của chính mình nếu là Owner
        if (IsOwner)
        {
            foreach (var item in meshToDisable)
            {
                item.enabled = false;
            }
        }
    }

     void Update()
    {
        {
            if (IsOwner)
            {
                
                    root.position = VRRigReferences.Singleton.root.position;
                    root.rotation = VRRigReferences.Singleton.root.rotation;

                head.position = VRRigReferences.Singleton.head.position;
                head.rotation = VRRigReferences.Singleton.head.rotation;

                leftHand.position = VRRigReferences.Singleton.leftHand.position;
                    leftHand.rotation = VRRigReferences.Singleton.leftHand.rotation;

                    rightHand.position = VRRigReferences.Singleton.rightHand.position;
                    rightHand.rotation = VRRigReferences.Singleton.rightHand.rotation;
                
            }
        }
    }
}
