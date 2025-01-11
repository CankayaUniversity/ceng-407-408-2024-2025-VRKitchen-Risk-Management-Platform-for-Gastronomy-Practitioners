using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ControlIconSelector", menuName = "Accessibility/Control Icons Selector")]
public class ControlIconSelector : ScriptableObject
{
    [System.Serializable]
    public class Mapping
    {
        public string Path;
        public Sprite Icone;
        public Color Color = Color.black;
    }
    
    [System.Serializable]
    public class Set
    {
        public string Name;
        public Mapping[] Mappings;
    }

    public Set[] MappingSets;

    //runtime constructed faster lookup table
    private Dictionary<string, Dictionary<string, Mapping>> m_LookupTables;
    
    public void Init()
    {
        m_LookupTables = new Dictionary<string, Dictionary<string, Mapping>>();
        foreach (var set in MappingSets)
        {
            Dictionary<string, Mapping> setMapping = new Dictionary<string, Mapping>();
            foreach (var mapping in set.Mappings)
            {
                setMapping.Add(mapping.Path, mapping);
            }
            
            m_LookupTables.Add(set.Name, setMapping);
        }
    }

    public List<string> GetSetList()
    {
        return m_LookupTables.Keys.ToList();
    }

    public Mapping GetMappingForSetAndPath(string set, string path)
    {
        if (m_LookupTables.TryGetValue(set, out var setMapping))
        {
            if (setMapping.TryGetValue(path, out var mapping))
            {
                return mapping;
            }
        }

        return null;
    }
}
