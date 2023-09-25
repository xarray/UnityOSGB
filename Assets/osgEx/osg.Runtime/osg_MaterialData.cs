using UnityEngine;

namespace osgEx
{
    [CreateAssetMenu(fileName = "default", menuName = "osg_MaterialSO")]
    public class osg_MaterialData : ScriptableObject
    {
        [SerializeField]
        private Material m_material;
        [SerializeField]
        private string m_mainTexProperty;
        [SerializeField]
        private string m_ambientColorProperty;
        [SerializeField]
        private string m_diffuseColorProperty;
        [SerializeField]
        private string m_specularColorProperty;
        [SerializeField]
        private string m_emissionColorProperty;

        public Material Material { get => m_material; }
        public string MainTexProperty { get => m_mainTexProperty; }
        public string AmbientColorProperty { get => m_ambientColorProperty; }
        public string DiffuseColorProperty { get => m_diffuseColorProperty; }
        public string SpecularColorProperty { get => m_specularColorProperty; }
        public string EmissionColorProperty { get => m_emissionColorProperty; }
    }
}
