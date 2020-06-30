using UnityEngine;
using System.Linq;

public class Group : MonoBehaviour
{
    public string[] GroupsName => groupName;

    [SerializeField] string[] groupName;


    public bool IsContaingAnyElement(Group testedGroup)
    {
        return testedGroup.GroupsName.Any(x => groupName.Contains(x));
    }

    public bool IsContaingAnyElement(string[] names)
    {
        return names.Any(x => groupName.Contains(x));
    }
}
