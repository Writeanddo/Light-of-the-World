using UnityEditor;

[CustomEditor(typeof(Torch))]
public class CustomerTorchEditor :Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var torch = (Torch)target;
        torch.UpdateBasedOnState();

        // Might be dirty but what the heck? it's a jam :P
        torch.LightBall.UpdateBasedOnState();
    }
}