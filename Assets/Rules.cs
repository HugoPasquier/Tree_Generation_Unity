using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rules : MonoBehaviour
{
    /*symmbols
    F: Move forward,drawing a line
    +,-: turn left/right
    [,]: push/pop current state
    */
    public IDictionary<string, List<string>> Presets = new Dictionary<string, List<string> >();
    
    //find the first rule matching the character
    List<string> findMatchingRule(char c)
    {
        /*
        foreach (KeyValuePair<string, List<List<string>> > entry in Presets)
        {
            foreach(List<string> rule in entry.Value)
            {
                if (c.ToString() == rule[0])
                {
                    return rule;
                }
            }

        }*/
        List<string> rules = new List<string>();
        if(Presets.TryGetValue(c.ToString(), out rules))
        {
            return rules;
        }
        return null;
    }

    //apply rules to sentence
    string ApplyRulesToSentence (string sentence)
    {
        string newSentence = "";
        List<string> rule = new List<string>();
        
        for (int i = 0; i < sentence.Length; i++)
        {
           char c= sentence[i];
            rule = findMatchingRule(c);
            if (rule !=null)
            {
                return rule[0];
            }
            else
            {
                newSentence += c;
            }

        }
        return newSentence;
    }

    //take an axiom and an iterator to return a sentence
    string ApplyRules(string axiom,int iterator)
    {
        string sentence=axiom;
        for(int i = 0; i < iterator; i++)
        {
            sentence= ApplyRulesToSentence(sentence);
        }
        return sentence;
    }

    void Start()
    {
        Presets.Add("X",new List<string> {
            "F+[-F-XF-X][+FF][--XF[+X]][++F-X]"
            });

        print(ApplyRules("X", 5));
    }

}
