using System.Collections.Generic;
using UnityEngine;

public class MaterialSwitcher : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private int defaultMaterialIndex;
    [SerializeField] private Material defaultMat;
    [SerializeField] private List<Materials> availableMaterials;

    private void Awake()
    {
        if(meshRenderer == null) TryGetComponent(out meshRenderer);
        
        if(defaultMat == null) defaultMat = meshRenderer.sharedMaterial;
        if(availableMaterials.Find(x => x.MaterialID == defaultMat.name) is null) 
            availableMaterials.Add(new Materials() { Mat = defaultMat });
    }

    public void SwitchMaterial(string matID)
    {
        if(meshRenderer is null) return;

        var sharedMats = meshRenderer.sharedMaterials;
        var newMat = availableMaterials.Find(x => x.MaterialID == matID);
        sharedMats[defaultMaterialIndex] = newMat is null ? sharedMats[defaultMaterialIndex] : newMat.Mat;
        
        meshRenderer.sharedMaterials = sharedMats;
    }

    public void SwitchMaterial(string matID, int matIndex = -1)
    {
        if(meshRenderer is  null) return;

        var sharedMats = meshRenderer.sharedMaterials;
        matIndex = matIndex < 0 ? defaultMaterialIndex : matIndex;

        var newMat = availableMaterials.Find(x => x.MaterialID == matID);
        sharedMats[matIndex] = newMat is null ? sharedMats[matIndex] : newMat.Mat;
        
        meshRenderer.sharedMaterials = sharedMats;
    }

    public void SwitchMaterial(Material mat) 
    {
        if(meshRenderer is null) return;

        var sharedMats = meshRenderer.sharedMaterials;
        sharedMats[defaultMaterialIndex] = mat is null ? sharedMats[defaultMaterialIndex] : mat;
        
        meshRenderer.sharedMaterials = sharedMats;
    }

    public void SwitchMaterial(Material mat, int matIndex = -1) 
    {
        if(meshRenderer is null) return;
        
        var sharedMats = meshRenderer.sharedMaterials;
        matIndex = matIndex < 0 ? defaultMaterialIndex : matIndex;        
        sharedMats[matIndex] = mat is null ? sharedMats[matIndex] : mat;
        
        meshRenderer.sharedMaterials = sharedMats;
    }

    public void SwitchMaterial(int index)
    {
        if(meshRenderer is null) return;

        var sharedMats = meshRenderer.sharedMaterials;

        if(index >= 0 && index < availableMaterials.Count)
            sharedMats[defaultMaterialIndex] = availableMaterials[index].Mat;

        meshRenderer.sharedMaterials = sharedMats;
    }

    public void SwitchMaterial(int index, int matIndex = -1)
    {
        if(meshRenderer is null) return;

        var sharedMats = meshRenderer.sharedMaterials;

        matIndex = matIndex < 0 ? defaultMaterialIndex : matIndex;

        if(index >= 0 && index < availableMaterials.Count)
            sharedMats[matIndex] = availableMaterials[index].Mat;

        meshRenderer.sharedMaterials = sharedMats;
    }

    public bool CompareMaterial(Material mat)
    {
        if(mat is null || meshRenderer is null) return false;

        return mat.name == meshRenderer.sharedMaterials[defaultMaterialIndex].name;
    }

    public bool CompareMaterial(Material mat, int matIndex = -1)
    {
        if(mat is null || meshRenderer is null) return false;

        matIndex = matIndex < 0 ? defaultMaterialIndex : matIndex;

        return mat.name == meshRenderer.sharedMaterials[matIndex].name;
    }

    public bool CompareMaterial(int index)
    {
        if(index < 0 || index >= availableMaterials.Count || meshRenderer is null) return false;

        return availableMaterials[index].MaterialID == meshRenderer.sharedMaterials[defaultMaterialIndex].name;
    }

    public bool CompareMaterial(int index, int matIndex = -1)
    {
        if(index < 0 || index >= availableMaterials.Count || meshRenderer is null) return false;

        matIndex = matIndex < 0 ? defaultMaterialIndex : matIndex;

        return availableMaterials[index].MaterialID == meshRenderer.sharedMaterials[matIndex].name;
    }

    public void ResetMaterial() => meshRenderer.sharedMaterials[defaultMaterialIndex] = defaultMat;

    public void ResetMaterial(int matIndex = -1)
    {
        matIndex = matIndex < 0 ? defaultMaterialIndex : matIndex;
        meshRenderer.sharedMaterials[matIndex] = defaultMat;
    }

    [System.Serializable]
    public class Materials
    {
        public string MaterialID => mat.name;

        [SerializeField] private Material mat;
        public Material Mat
        {
            get => mat;
            set => mat = value;
        }
    }
}
