using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof (NavMeshAgent))]

public class Enemy : LivingEntity {

	public enum State{IDLE,CHASING,ATTACKING};
	State currentState;

	public ParticleSystem deathEffect;
	public static event System.Action OnDeathStatic;

	NavMeshAgent pathFinder;
	Transform target;
	LivingEntity targetEntity;
	Material skinMaterial;

	Color originalColour;

	float attackDistanceThreshold = 0.5f;
	float timeBetweenAttacks = 1;
	float damage = 1;

	float nextAttackTime;
	float myCollisionRadius;
	float targetCollisionRadius;

	bool hasTarget;

	void Awake() {
		pathFinder = GetComponent<NavMeshAgent> ();

		if (GameObject.FindGameObjectWithTag ("Player") != null) {
			hasTarget = true;

			target = GameObject.FindGameObjectWithTag ("Player").transform;
			targetEntity = target.GetComponent<LivingEntity> ();

			myCollisionRadius = GetComponent<CapsuleCollider> ().radius;
			targetCollisionRadius = target.GetComponent<CapsuleCollider> ().radius;
		}
	}

	protected override void Start () {
		base.Start ();

		if (hasTarget) {
			currentState = State.CHASING;
			targetEntity.OnDeath += OnTargetDeath; 	

			StartCoroutine (UpdatePath ());
		}
	}

	public void SetCharacteristics(float moveSpeed, int hitsToKillplayer, float enemyHealth, Color skinColor ) {
		pathFinder.speed = moveSpeed;

		if (hasTarget) {
			damage = Mathf.Ceil( targetEntity.startingHealth / hitsToKillplayer);
		}
		startingHealth = enemyHealth;

		deathEffect.startColor = new Color (skinColor.r, skinColor.g, skinColor.b, 1);
		skinMaterial = GetComponent<Renderer> ().material;
		skinMaterial.color = skinColor;
		originalColour = skinMaterial.color;

	}

	public override void TakeHit (float damage, Vector3 hitPoint, Vector3 hitDirection)
	{
		AudioManager.instance.PlaySound ("Impact", transform.position);
		if (damage >= health) {
			if (OnDeathStatic != null) {
				OnDeathStatic ();
			}
			AudioManager.instance.PlaySound ("Enemy Death", transform.position);
			Destroy(Instantiate (deathEffect.gameObject, hitPoint, Quaternion.FromToRotation (Vector3.forward, hitDirection)) as GameObject, deathEffect.main.startLifetimeMultiplier);
		}
		base.TakeHit (damage, hitPoint, hitDirection);
	}

	void OnTargetDeath(){
		hasTarget = false;
		currentState = State.IDLE;
	}

	void Update () {

		if (hasTarget) {
			if (Time.time > nextAttackTime) {
				float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
				if (sqrDstToTarget < Mathf.Pow (attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)) {
					nextAttackTime = Time.time + timeBetweenAttacks;
					AudioManager.instance.PlaySound ("Enemy Attack", transform.position);
					StartCoroutine (Attack ());
				}
			}
		}
	}

	IEnumerator Attack() {

		currentState = State.ATTACKING;
		pathFinder.enabled = false;

		Vector3 orginialPosition = transform.position;
		Vector3 directionToTarget = (target.position - transform.position).normalized;
		Vector3 attackPosition = target.position - directionToTarget * (myCollisionRadius);

		float attackSpeed = 3;
		float percent = 0;

		skinMaterial.color = Color.red;
		bool hasAppliedDamage = false;

		while (percent <= 1) {

			if (percent >= 0.5f && !hasAppliedDamage) {
				hasAppliedDamage = true;
				targetEntity.TakeDamage (damage);
			}
			percent += Time.deltaTime * attackSpeed;
			float interpolation = (-Mathf.Pow (percent, 2) + percent) * 4;
			transform.position = Vector3.Lerp (orginialPosition, attackPosition, interpolation);

			yield return null;
		}

		skinMaterial.color = originalColour;

		currentState = State.CHASING;
		pathFinder.enabled = true;
	}	

	IEnumerator UpdatePath() {
		float refreshRate = 0.25f;

		while (hasTarget) {
			if (currentState == State.CHASING) {
				Vector3 directionToTarget = (target.position - transform.position).normalized;
				Vector3 targetPosition = target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);
				if (!dead)
					pathFinder.SetDestination (targetPosition);
			}
			yield return new WaitForSeconds (refreshRate);
		}
	}
}
