using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{

    public Transform Player;
    public float speed;
    public float rotspeed;
    public float heightspeed;
    public float height;
    public float attackspeed;
    public AnimationCurve attackcurve;

    private Vector3 Velocity;
    private Vector3 Circle;
    private bool isattack = false;
    private Vector3 AttackTarget;
    private float AttackWeight = 0.0f;
    private Vector3 InitialAttackPos;
    private float attackcolddown = 0.0f;
    
    // Start is called before the first frame update
    void Start()
    {        
        Circle = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        

        Vector3 PlayerInTargetSpaceDir = transform.InverseTransformPoint(Player.transform.position).normalized;
        if ((PlayerInTargetSpaceDir.z > 0) && (Mathf.Abs(PlayerInTargetSpaceDir.x / PlayerInTargetSpaceDir.z) <0.5f))
        {
            Debug.Log("attack");
            if ((!isattack)  &&  (AttackWeight == 0.0f))
            {
                isattack = true;
                AttackTarget = Player.position;
                InitialAttackPos = transform.position + new Vector3(0.0f, 1.0f, 0.0f);
            }
                
        }

        
        if (Vector3.Distance(transform.position , Player.position) > 6.0f)
        {
            AttackTarget = Player.position + new Vector3(0.0f, 1.0f, 0.0f);
        }

        if (isattack)
        {
            AttackWeight += Time.deltaTime * attackspeed;

        }
        else
        {
            AttackWeight -= Time.deltaTime * attackspeed * 0.5f;
        }


        AttackWeight = Mathf.Clamp01(AttackWeight);

        //Circle = new Vector3(Mathf.Sin(Time.time) * 20.0f, 18.0f + Mathf.PerlinNoise(Time.time * heightspeed, 0.1f) * height, Mathf.Cos(Time.time) * 20.0f);
        Velocity = (transform.forward + transform.right * rotspeed).normalized * speed * Time.deltaTime;
        Circle = transform.position + Velocity;
        //Circle.y = 18.0f + Mathf.PerlinNoise(Time.time * heightspeed, 0.1f) * height;

        Vector3 finaltarget;

        if (isattack)
        {
            //Vector3 attackpos = Vector3.Lerp(AttackTarget;
            finaltarget = Vector3.Lerp(InitialAttackPos, AttackTarget, AttackWeight);
        }
        else
        {
            //finaltarget = Vector3.Lerp(Circle, InitialAttackPos, AttackWeight);           
            float y = Mathf.Lerp(18.0f + Mathf.PerlinNoise(Time.time * heightspeed, 0.1f) * height, InitialAttackPos.y, AttackWeight);
            Circle.y = y;
            finaltarget = Circle;
        }


        Vector3 dir = (finaltarget - transform.position).normalized;
        //Vector3 right = Vector3.Cross(Vector3.up, dir);

        //transform.rotation =  Quaternion.FromToRotation(transform.forward, dir) * transform.rotation;
        transform.LookAt(finaltarget);

        transform.position = finaltarget;

        if (AttackWeight >= 1.0f)
        {
            isattack = false;
            InitialAttackPos = transform.position;
        }


    }
}
