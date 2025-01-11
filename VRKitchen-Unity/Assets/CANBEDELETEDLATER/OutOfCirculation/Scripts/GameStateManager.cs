using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class behaves very similarly to a dictionary but is a persistent singleton which instantiates itself when needed.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    List<string> m_Keys = new List<string>();
    List<bool> m_Values = new List<bool>();

    static GameStateManager s_Instance;
    
    public static GameStateManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                GameObject gameObject = new GameObject("GameStateManager");
                s_Instance = gameObject.AddComponent<GameStateManager>();
                DontDestroyOnLoad(gameObject);
            }
            
            return s_Instance;
        }
        private set
        {
            s_Instance = value;
        }
    }

    public void RemoveVariable(string variable)
    {
        for (int i = 0; i < m_Keys.Count; i++)
        {
            if (m_Keys[i] == variable)
            {
                m_Keys.RemoveAt(i);
                m_Values.RemoveAt(i);
                return;
            }
        }

        throw new UnityException(variable + " was not found in the GameStateManager and so could not be removed.");
    }

    public bool Contains(string variable)
    {
        for (int i = 0; i < m_Keys.Count; i++)
        {
            if (m_Keys[i] == variable)
            {
                return true;
            }
        }

        return false;
    }

    public bool GetValue(string variable)
    {
        for (int i = 0; i < m_Keys.Count; i++)
        {
            if (m_Keys[i] == variable)
            {
                return m_Values[i];
            }
        }

        throw new UnityException(variable + " was not found in the GameStateManager and so its value cannot be returned.");
    }

    public bool TryGetValue(string variable, out bool value)
    {
        for (int i = 0; i < m_Keys.Count; i++)
        {
            if (m_Keys[i] == variable)
            {
                value = m_Values[i];
                return true;
            }
        }

        value = false;
        return false;
    }

    public void SetValue(string variable, bool value)
    {
        for (int i = 0; i < m_Keys.Count; i++)
        {
            if (m_Keys[i] == variable)
            {
                m_Values[i] = value;
                return;
            }
        }
        
        m_Keys.Add(variable);
        m_Values.Add(value);
    }
}
