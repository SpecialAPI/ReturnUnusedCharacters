using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderTester : MonoBehaviour
{
    public Material objectMaterial;
    public Material cameraMaterial;

    public List<Renderer> rendererMaterials = new List<Renderer>();

    public void Awake()
    {
        if(objectMaterial != null)
        {
            var renderers = FindObjectsOfType<Renderer>();

            foreach(var rend in renderers)
            {
                var tex = rend.material.mainTexture;

                rend.material = /*new Material(*/objectMaterial/*)*/;

                //rend.material.mainTexture = tex;

                rendererMaterials.Add(rend);
            }
        }
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (cameraMaterial)
        {
            Graphics.Blit(source, destination, cameraMaterial);
        }
    }

    public void OnGUI()
    {
        if(GUI.Button(new Rect(10, 10, 200, 50), "Update materials"))
        {
            foreach(var rend in rendererMaterials)
            {
                var tex = rend.material.mainTexture;

                rend.material = new Material(objectMaterial);

                rend.material.mainTexture = tex;
            }
        }

        if(GUI.Button(new Rect(10, 70, 200, 50), "Enable keyword"))
        {
            objectMaterial.EnableKeyword("FANCY_STUFF_ON");
            objectMaterial.DisableKeyword("FANCY_STUFF_OFF");
        }

        if (GUI.Button(new Rect(10, 130, 200, 50), "Disable keyword"))
        {
            objectMaterial.DisableKeyword("FANCY_STUFF_ON");
            objectMaterial.EnableKeyword("FANCY_STUFF_OFF");
        }
    }
}
