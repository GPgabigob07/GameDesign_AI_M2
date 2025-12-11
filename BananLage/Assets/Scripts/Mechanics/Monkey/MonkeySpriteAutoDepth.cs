using System;
using System.Collections;
using System.Collections.Generic;
using Structures;
using TMPro;
using UnityEngine;

namespace Mechanics.Monkey
{
    public class MonkeySpriteAutoDepth : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        [SerializeField] private float radius, frames;
        [SerializeField] private LayerMask strutureLayer;

        private WaitForSeconds _delay;

        private void Start()
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            frames = Mathf.Max(1, frames);
            _delay = new WaitForSeconds(frames * 0.016667f);
        }

        private void OnEnable()
        {
            StartCoroutine(CheckAndAdjustSprite());
        }

        private IEnumerator CheckAndAdjustSprite()
        {
            while (true)
            {
                var target = Physics2D.OverlapCircle(transform.position, radius, strutureLayer);
                if (target && target.TryGetComponent<BaseStructure>(out var structure))
                {
                    var sy = target.transform.position.y;
                    var my = transform.position.y;
                    var order = structure.SpriteRenderer.sortingOrder;

                    _spriteRenderer.sortingOrder = order + (my > sy ? -3 : 3);
                }

                yield return _delay;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}