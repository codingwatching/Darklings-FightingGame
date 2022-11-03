﻿using Demonics.Enum;
using Demonics.Utility;
using System;
using FixMath.NET;
using UnityEngine;

public class Hitbox : DemonicsCollider
{
    public Action OnGroundCollision;
    public Action OnPlayerCollision;
    [SerializeField] private bool _hitGround;
    [SerializeField] private IHitboxResponder _hitboxResponder;
    private Transform _sourceTransform;
    public Transform HitPoint { get; private set; }
    public bool HitConfirm { get; private set; }


    void Awake()
    {
        _sourceTransform = transform.root;
    }

    protected override void Start()
    {
        base.Start();
        if (_hitboxResponder == null)
        {
            _hitboxResponder = transform.root.GetComponent<IHitboxResponder>();
        }
    }

    protected override void InitializeCollisionList()
    {
        DemonicsCollider[] demonicsCollidersArray = FindObjectsOfType<DemonicsCollider>();
        for (int i = 0; i < demonicsCollidersArray.Length; i++)
        {
            if (!demonicsCollidersArray[i].transform.IsChildOf(transform.root))
            {
                if (demonicsCollidersArray[i].TryGetComponent(out Hurtbox hurtbox))
                {
                    _demonicsColliders.Add(demonicsCollidersArray[i]);
                }
            }
        }
        _demonicsColliders.Remove(this);
    }

    public void SetSourceTransform(Transform sourceTransform)
    {
        _sourceTransform = sourceTransform;
    }

    public void SetHitboxResponder(Transform hitboxResponder)
    {
        _hitboxResponder = hitboxResponder.GetComponent<IHitboxResponder>();
    }

    public void SetBox(Vector2 size, Vector2 offset)
    {
        Vector2 offsetWorldSpace = transform.TransformPoint(offset);
        Size = new FixVector2((Fix64)size.x, (Fix64)size.y);
        Offset = new FixVector2((Fix64)offset.x, (Fix64)offset.y);
    }

    protected override void EnterCollision(DemonicsCollider collider)
    {
        base.EnterCollision(collider);
        if (collider.TryGetComponent(out Hurtbox hurtbox))
        {
            _hitboxResponder.HitboxCollided(Vector2.zero, hurtbox);
        }
    }

    protected override void ExitCollision()
    {
        base.ExitCollision();
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        GizmoColor = Color.red;
        base.OnDrawGizmos();
    }
#endif
}