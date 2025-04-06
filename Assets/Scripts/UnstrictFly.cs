using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnstrictFly : MonoBehaviour
{

    [System.Serializable]
    public class Chain
    {
        public Transform point;
        public Vector3 OriginalPosition;
        public Quaternion OriginalRotation = Quaternion.identity;
        public float Length = 0.0f;
        public Quaternion rotationFix = Quaternion.identity;
    }

    public Transform Target;

    public Transform Root;
    public Transform Direction;

    public List<Chain> Chains = new List<Chain>();
    public List<Transform> Bones = new List<Transform>();
    

    private Quaternion LastTargetRotation = Quaternion.identity;
    private GameObject ChainRoot;

    // Start is called before the first frame update
    void Start()
    {
        ChainRoot = new GameObject("ChainsGroup");
        
        Chains.Clear();
        Bones.Clear();

        if (Root != null)
        {            
            SetchildTransform(Root);
        }
            

        if (Chains.Count > 1)
        {
            for (int index = 1; index < Chains.Count; index++)
            {
                Chains[index].Length = Vector3.Distance(Chains[index - 1].point.position, Chains[index].point.position);
                
            }
        }

        if (Target != null)
        {
            LastTargetRotation = Target.rotation;
        }
    }

    private void SetchildTransform(Transform parent)
    {
        GameObject sphere = new GameObject("chain");
        sphere.transform.parent = ChainRoot.transform;
        sphere.name = parent.name;
        sphere.transform.position = parent.position;

        Chain newchain = new Chain();
        newchain.point = sphere.GetComponent<Transform>();
        newchain.OriginalPosition = parent.position;
        newchain.OriginalRotation = parent.rotation;
        newchain.rotationFix = Quaternion.Inverse(Direction.transform.rotation) * parent.rotation;

        Chains.Add(newchain);
        Bones.Add(parent);

        if (parent.GetComponentsInChildren<Transform>().Length > 1)
        {
            Transform child = parent.GetChild(0);

            SetchildTransform(child);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        //记录骨骼的世界坐标
        for (int index = 0; index < Chains.Count; index++)
        {
            Chains[index].OriginalPosition = Bones[index].position;            
            Chains[index].OriginalRotation = Bones[index].rotation;
        }

        //Unstrict
        Chains[0].point.position = Target.position;
        Chains[0].point.rotation = Target.rotation * Quaternion.Inverse(LastTargetRotation);
        for (int index = 1; index < Chains.Count; index++)
        {

            Vector3 DirectionToPrevious_Original = (Chains[index - 1].OriginalPosition - Chains[index].OriginalPosition).normalized;
            Vector3 DirectionToPrevious = (Chains[index - 1].point.position - Chains[index].point.position).normalized;
            Vector3 TargetLocation = Chains[index - 1].point.position - DirectionToPrevious * Chains[index].Length;

            Chains[index].point.position = TargetLocation;

            //Chains[index].point.rotation = Quaternion.FromToRotation(DirectionToPrevious_Original, DirectionToPrevious);
            Chains[index].point.transform.LookAt(Chains[index - 1].point.position);  
        }

        LastTargetRotation = Target.rotation;


        Bones[0].position = Chains[0].point.position;
        Bones[0].rotation = Chains[0].point.rotation * Chains[0].OriginalRotation;
        for (int index = 1; index < Bones.Count; index++)
        {

            Bones[index].position = Chains[index].point.position;
            //Bones[index].rotation = Chains[index].point.rotation * Chains[index].OriginalRotation;
            Bones[index].rotation = Chains[index].point.rotation * Chains[index].rotationFix;

            /*if (index > 0)
            {
                //lock rotate x axis
                Vector3 angle = Bones[index].localEulerAngles;
                angle.x = 0.0f;
                Bones[index].localEulerAngles = angle;
            }*/


        }

    }
}
