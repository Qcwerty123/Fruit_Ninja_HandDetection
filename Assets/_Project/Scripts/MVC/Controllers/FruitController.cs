using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FruitController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Collider2D _collider;
    
    private IObjectPool<FruitController> _pool;
    private GameModel _gameModel;
    private FruitData _data;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }

    public void Setup(FruitData data, IObjectPool<FruitController> pool, GameModel gameModel)
    {
        _data = data;
        _pool = pool;
        _gameModel = gameModel;
        _collider.enabled = true; 
    }

    public void Launch(Vector2 position, Vector2 force, float torque)
    {
        transform.position = position;
        
        // Zero-Allocation Physics Reset
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        
        _rb.AddForce(force, ForceMode2D.Impulse);
        _rb.AddTorque(torque, ForceMode2D.Impulse);
    }

    public void Slice(Vector2 cutDirection)
    {
        _collider.enabled = false;

        if (_data.slicedPrefab != null)
        {
            // TODO (Epic 5): Tối ưu Pool cho mảnh vỡ
            GameObject slicedFruit = Instantiate(_data.slicedPrefab, transform.position, transform.rotation);
            
            Vector2 perpendicularDir = new Vector2(-cutDirection.y, cutDirection.x).normalized;
            Rigidbody2D[] slices = slicedFruit.GetComponentsInChildren<Rigidbody2D>();
            
            if (slices.Length >= 2)
            {
                slices[0].AddForce(perpendicularDir * 4f, ForceMode2D.Impulse);
                slices[1].AddForce(-perpendicularDir * 4f, ForceMode2D.Impulse);
            }
            
            Destroy(slicedFruit, 3f);
        }

        if (_data.juiceParticle != null)
        {
            // TODO (Epic 5): Tối ưu Pool cho hiệu ứng hạt
            GameObject juice = Instantiate(_data.juiceParticle, transform.position, Quaternion.identity);
            Destroy(juice, 2f);
        }

        if (_gameModel != null && !_data.isBomb)
        {
            _gameModel.AddScore(_data.scoreValue);
        }

        Despawn();
    }

    public void Despawn()
    {
        if (gameObject.activeSelf && _pool != null)
        {
            _pool.Release(this);
        }
    }
}