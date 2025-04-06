using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loong : MonoBehaviour
{

    [System.Serializable]
    public class Chain
    {
        public Transform point;
        public Vector3 OriginalPosition;
        public Quaternion OriginalRotation = Quaternion.identity;
        public float Length = 0.0f;
        
    }

    [System.Serializable]
    public class Path
    {
        public Vector3 Position;
        public Quaternion Rotation = Quaternion.identity;
    }

    public bool isstrictpath = false;
    public Transform Root;
    public List<Chain> Chains = new List<Chain>();
    public List<Transform> Bones = new List<Transform>();
    public Transform Target;
    private Quaternion LastTargetRotation = Quaternion.identity;

    public List<Path> RecordPath = new List<Path>();
    //����·����ľ���
    public float Precision = 1.0f;
    //��·�����ȳ�������ֵʱ���Ƴ�·����
    private float MaximumBoneExtension = 0.0f;
    //��¼·���������
    public int MaximumPathStep = 50;
    private Quaternion TargetInitialRotation;

    // Start is called before the first frame update
    void Start()
    {
        Chains.Clear();
        Bones.Clear();
        
        //��ʼ������
        if (Root != null)
            SetchildTransform(Root);

        //��¼��������
        if (Chains.Count > 1)
        {
            for (int index = 1; index < Chains.Count; index++)
            {
                Chains[index].Length = Vector3.Distance(Chains[index-1].point.position,Chains[index].point.position);
                MaximumBoneExtension += Chains[index].Length;
            }
        }

        if (Target != null)
        {
            LastTargetRotation = Target.rotation;
        }

        //��ʼ��·������
        if (isstrictpath)
        {
            RecordPath.Clear();

            for (int index = Bones.Count - 1; index > -1; index--)
            {
                Path oripoint = new Path();
                oripoint.Position = Bones[index].transform.position;
                //oripoint.Rotation = Bones[index].transform.rotation;
                RecordPath.Add(oripoint);
            }

        }

        TargetInitialRotation = Target.rotation;

    }

    private void SetchildTransform(Transform parent)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = parent.position;        

        Chain newchain = new Chain();
        newchain.point = sphere.GetComponent<Transform>();
        newchain.OriginalPosition = parent.position;
        newchain.OriginalRotation = parent.rotation;        

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
        //��¼��������������
        for (int index = 0; index < Chains.Count; index++)
        {
            Chains[index].OriginalPosition = Bones[index].position;
            if ((!isstrictpath) || (index == 0))
                Chains[index].OriginalRotation = Bones[index].rotation;
        }

        if (isstrictpath)
        {
            //Strict
            Chains[0].point.position = Target.position;
            Chains[0].point.rotation = Target.rotation * Quaternion.Inverse(LastTargetRotation);


            //�ۼӵ���ǰ�����ڵ���ܳ���
            float DesiredDistance = 0;                       
            

            if (Vector3.Distance(RecordPath[RecordPath.Count - 1].Position, Target.position) > Precision)
            {
                Path oripoint = new Path();
                oripoint.Position = Target.position;
                oripoint.Rotation = Target.rotation * Quaternion.Inverse(TargetInitialRotation);
                RecordPath.Add(oripoint);

                if (RecordPath.Count > MaximumPathStep)
                {
                    RecordPath.RemoveAt(0);
                }
            }

            for (int index = 1; index < Chains.Count; index++)
            {
                DesiredDistance += Chains[index].Length;

                //·�����ۼӳ���
                float AccumulatedDistance = 0.001f;

                Vector3 TargetPositionForCurrentBone = Vector3.zero;
                Quaternion TargetRotationForCurrentBone = Quaternion.identity;

                for (int PathIndex = RecordPath.Count - 2; PathIndex > -1; PathIndex--)
                {
                    //FTransform PathPoint = RecordedPath[PathIndex];
                    //AccumulatedDistance += (RecordedPath[PathIndex + 1].GetLocation() - PathPoint.GetLocation()).Size();
                    AccumulatedDistance += Vector3.Distance(RecordPath[PathIndex + 1].Position, RecordPath[PathIndex].Position);

                    //FVector Chain_Distance_Diff = ((InOutChain[i].OriginalPosition).GetLocation() - (InOutChain[i + 1].OriginalPosition).GetLocation());
                    //��ǰ�ڵ�ָ����һ������
                    Vector3 ChainDir = Chains[index].OriginalPosition - Chains[index-1].OriginalPosition;

                    //��·�������ۼӴ��ڹ��������ۼ�ʱ
                    if (AccumulatedDistance >= DesiredDistance)
                    {
                        float lerp_segment = Mathf.Clamp(1 - ((RecordPath[RecordPath.Count-1].Position - Target.position).magnitude / Precision), 0, 1);

                        if ((PathIndex - 1) > -1)
                        {
                            //TargetPositionForCurrentBone.SetLocation(InOutChain[i + 1].Position.GetLocation() - (InOutChain[i + 1].Position.GetLocation() - UKismetMathLibrary::VLerp(RecordedPath[PathIndex].GetLocation(), RecordedPath[PathIndex - 1].GetLocation(), lerp_segment)).GetUnsafeNormal() * Chain_Distance_Diff.Size());
                            //TargetPositionForCurrentBone.SetRotation(UKismetMathLibrary::Quat_Slerp(RecordedPath[PathIndex].GetRotation(), RecordedPath[PathIndex - 1].GetRotation(), lerp_segment));
                            //TargetPositionForCurrentBone.SetScale3D(UKismetMathLibrary::VLerp(RecordedPath[PathIndex].GetScale3D(), RecordedPath[PathIndex - 1].GetScale3D(), lerp_segment));
                            TargetPositionForCurrentBone = Chains[index - 1].point.position - (Chains[index - 1].point.position - Vector3.Lerp(RecordPath[PathIndex].Position, RecordPath[PathIndex - 1].Position, lerp_segment)).normalized * ChainDir.magnitude;
                            TargetRotationForCurrentBone = Quaternion.Slerp(RecordPath[PathIndex].Rotation, RecordPath[PathIndex - 1].Rotation, lerp_segment);
                        }
                        else
                        {
                            //TargetPositionForCurrentBone.SetLocation(InOutChain[i + 1].Position.GetLocation() - (InOutChain[i + 1].Position.GetLocation() - RecordedPath[PathIndex].GetLocation()).GetUnsafeNormal() * Chain_Distance_Diff.Size());
                            //TargetPositionForCurrentBone.SetRotation(RecordedPath[PathIndex].GetRotation());
                            //TargetPositionForCurrentBone.SetScale3D(RecordedPath[PathIndex].GetScale3D());
                            TargetPositionForCurrentBone = Chains[index - 1].point.position - (Chains[index - 1].point.position - RecordPath[PathIndex].Position).normalized * ChainDir.magnitude;
                            TargetRotationForCurrentBone = RecordPath[PathIndex].Rotation;

                        }


                        break;
                    }



                }

                Chains[index].point.position = TargetPositionForCurrentBone;
                Chains[index].point.rotation = TargetRotationForCurrentBone;

            }

        }
        else
        {
            //Unstrict
            //�ƶ���һ�����������ݹ������Ⱥ��ƶ���Ĺ�����������¼�������λ�ã�����ԭʼ����������ƶ���Ĺ���������ת�¼�����
            Chains[0].point.position = Target.position;
            Chains[0].point.rotation = Target.rotation * Quaternion.Inverse(LastTargetRotation);            
            for (int index = 1; index < Chains.Count; index++)
            {

                Vector3 DirectionToPrevious_Original = (Chains[index - 1].OriginalPosition - Chains[index].OriginalPosition).normalized;
                Vector3 DirectionToPrevious = (Chains[index - 1].point.position - Chains[index].point.position).normalized;
                Vector3 TargetLocation = Chains[index - 1].point.position - DirectionToPrevious * Chains[index].Length;

                Chains[index].point.position = TargetLocation;

                Chains[index].point.rotation = Quaternion.FromToRotation(DirectionToPrevious_Original, DirectionToPrevious);

            }
        }

        LastTargetRotation = Target.rotation;

        
        for (int index = 0; index < Bones.Count; index++)
        {
            
            Bones[index].position = Chains[index].point.position;
            Bones[index].rotation = Chains[index].point.rotation * Chains[index].OriginalRotation;
        }

    }

    
}
