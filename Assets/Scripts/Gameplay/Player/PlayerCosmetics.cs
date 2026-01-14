using UnityEngine;

public class PlayerCosmetics : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerProfile profile;
    [SerializeField] private ShipDatabase shipDatabase;

    [Header("Mount Points")]
    [SerializeField] private Transform skinRoot;
    [SerializeField] private Transform trailRoot;
    [SerializeField] private Transform coreFxRoot;

    [Header("Defaults")]
    [SerializeField] private bool hideDefaultVisualWhenSkinSelected = true;
    [SerializeField] private TrailRenderer defaultTrailRenderer;

    private PlayerVisual _playerVisual;
    private GameObject _skinInstance;
    private GameObject _trailInstance;
    private GameObject _coreFxInstance;

    private void Awake()
    {
        _playerVisual = GetComponent<PlayerVisual>();
        if (defaultTrailRenderer == null)
            defaultTrailRenderer = GetComponentInChildren<TrailRenderer>(true);
    }

    private void Start()
    {
        ApplyCosmetics();
    }

    public void ApplyCosmetics()
    {
        if (profile == null || shipDatabase == null)
            return;

        profile.EnsureDefaults(shipDatabase);

        ApplySkin(profile.SelectedSkinId);
        ApplyTrail(profile.SelectedTrailId);
        ApplyCoreFx(profile.SelectedCoreFxId);
    }

    private void ApplySkin(string skinId)
    {
        var skin = shipDatabase.GetSkin(skinId);
        if (skin == null || skin.prefab == null)
        {
            if (_skinInstance != null)
                Destroy(_skinInstance);

            _skinInstance = null;
            if (hideDefaultVisualWhenSkinSelected && _playerVisual != null)
                _playerVisual.SetVisible(true);
            return;
        }

        if (hideDefaultVisualWhenSkinSelected && _playerVisual != null)
            _playerVisual.SetVisible(false);

        if (_skinInstance != null)
            Destroy(_skinInstance);

        _skinInstance = Instantiate(skin.prefab, GetRootOrSelf(skinRoot));
    }

    private void ApplyTrail(string trailId)
    {
        var trail = shipDatabase.GetTrail(trailId);
        if (trail == null || trail.prefab == null)
        {
            if (_trailInstance != null)
                Destroy(_trailInstance);

            _trailInstance = null;
            if (defaultTrailRenderer != null)
                defaultTrailRenderer.enabled = true;
            return;
        }

        if (defaultTrailRenderer != null)
            defaultTrailRenderer.enabled = false;

        if (_trailInstance != null)
            Destroy(_trailInstance);

        _trailInstance = Instantiate(trail.prefab, GetRootOrSelf(trailRoot));
    }

    private void ApplyCoreFx(string coreFxId)
    {
        var coreFx = shipDatabase.GetCoreFx(coreFxId);
        if (coreFx == null || coreFx.prefab == null)
        {
            if (_coreFxInstance != null)
                Destroy(_coreFxInstance);

            _coreFxInstance = null;
            return;
        }

        if (_coreFxInstance != null)
            Destroy(_coreFxInstance);

        _coreFxInstance = Instantiate(coreFx.prefab, GetRootOrSelf(coreFxRoot));
    }

    private Transform GetRootOrSelf(Transform root)
    {
        return root != null ? root : transform;
    }
}
