using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab; // Prefab để sinh đối tượng
    [SerializeField] private List<GameObject> multiplePrefabs; // Danh sách các Prefab để chọn ngẫu nhiên
    [SerializeField] private int maxSize = 5; // Số lượng tối đa trong Pool

    private List<GameObject> pool = new List<GameObject>(); // Danh sách các đối tượng trong Pool

    #region 
    /// <summary>
    /// Khởi tạo Object Pool với số lượng đối tượng tối đa.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    void Start()
    {
        if (prefab == null && (multiplePrefabs == null || multiplePrefabs.Count == 0))
        {
            Debug.LogError("No prefab or multiple prefabs assigned to ObjectPool on " + gameObject.name);
            return;
        }
        InitializePool();
    }

    #region 
    /// <summary>
    /// Khởi tạo Pool với số lượng đối tượng tối đa.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    private void InitializePool()
    {
        for (int i = 0; i < maxSize; i++)
        {
            GameObject obj = CreateNewObject();
            obj.SetActive(false); // Ẩn đối tượng trong Pool
            pool.Add(obj);
        }
    }

    #region 
    /// <summary>
    /// Lấy một đối tượng ngẫu nhiên từ Pool, nếu hết thì tái sử dụng đối tượng cũ ngẫu nhiên.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    /// <param name="position">Vị trí để đặt đối tượng</param>
    /// <param name="rotation">Góc xoay của đối tượng</param>
    /// <returns>Đối tượng được kích hoạt</returns>
    #endregion
    public GameObject GetObject(Vector3 position, Quaternion rotation)
    {
        // Lấy danh sách các đối tượng chưa hoạt động
        List<GameObject> inactiveObjects = pool.Where(obj => !obj.activeInHierarchy).ToList();

        if (inactiveObjects.Count > 0)
        {
            // Chọn ngẫu nhiên một đối tượng từ danh sách inactive
            int randomIndex = Random.Range(0, inactiveObjects.Count);
            GameObject obj = inactiveObjects[randomIndex];
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            Debug.Log("Retrieved random pillar from pool: " + obj.name);
            return obj;
        }

        // Nếu không còn đối tượng trống, tái sử dụng một đối tượng ngẫu nhiên từ Pool
        int randomActiveIndex = Random.Range(0, pool.Count);
        GameObject reusedObj = pool[randomActiveIndex];
        reusedObj.SetActive(false); // Vô hiệu hóa trước để reset trạng thái
        Rigidbody rb = reusedObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Đặt lại kinematic
        }
        reusedObj.transform.position = position;
        reusedObj.transform.rotation = rotation;
        reusedObj.SetActive(true);
        Debug.Log("Reused random pillar: " + reusedObj.name);
        return reusedObj;
    }

    #region 
    /// <summary>
    /// Tạo một đối tượng mới từ prefab hoặc chọn ngẫu nhiên từ danh sách prefabs.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: N/A
    /// </summary>
    /// <returns>Đối tượng mới được tạo</returns>
    #endregion
    private GameObject CreateNewObject()
    {
        GameObject prefabToUse = prefab;
        if (multiplePrefabs != null && multiplePrefabs.Count > 0)
        {
            int randomIndex = Random.Range(0, multiplePrefabs.Count);
            prefabToUse = multiplePrefabs[randomIndex];
        }
        GameObject obj = Instantiate(prefabToUse, Vector3.zero, Quaternion.identity);
        obj.transform.parent = this.transform; // Gắn đối tượng vào Pool để quản lý
        return obj;
    }
}