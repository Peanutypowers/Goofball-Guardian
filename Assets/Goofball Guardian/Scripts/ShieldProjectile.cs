using UnityEngine;

public class ShieldProjectile : MonoBehaviour
{
 public float speed = 2f;
    public ShieldGameManager game; // set by the game manager when it spawns this
    bool blocked = false;
 Vector3 flyAwayDirection;
    void Update()
    {
        // round ended -get rid of leftover projectiles
     if (!game.playing)
        {
            Destroy(gameObject);
          return;
        }
        // got blocked -fly away until it gets destroyed
        if (blocked)
        {
          transform.position += flyAwayDirection * speed * 3 * Time.deltaTime;
            return;
        }

     Vector3 creaturePos = game.creature.position;

        if (game.shield.CanBlock(transform.position))
        {
          blocked = true;
            flyAwayDirection = (transform.position - creaturePos).normalized;
              game.Blocked();
            Destroy(gameObject, 0.5f);
          return;
        }

        if (Vector3.Distance(transform.position, creaturePos) < 0.55f)
        {
          game.CreatureHit();
            Destroy(gameObject);
          return;
        }
        // move toward the creature
     transform.position = Vector3.MoveTowards(transform.position, creaturePos, speed * Time.deltaTime);
    }
}