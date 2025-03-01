using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab; // Prefab để sinh đối tượng (có thể để trống nếu dùng multiplePrefabs)
    [SerializeField] private List<GameObject> multiplePrefabs; // Danh sách các Prefab để chọn ngẫu nhiên
    [SerializeField] private int initialSize = 10; // Kích thước Pool ban đầu

    private List<GameObject> pool = new List<GameObject>(); // Danh sách các đối tượng trong Pool

    #region 
    /// <summary>
    /// Khởi tạo Object Pool với số lượng đối tượng ban đầu.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: N/A
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
    /// Khởi tạo Pool với các đối tượng ban đầu dựa trên prefab hoặc danh sách prefabs.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: N/A
    /// </summary>
    #endregion
    private void InitializePool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject();
            obj.SetActive(false); // Ẩn đối tượng trong Pool
            pool.Add(obj);
        }
    }

    #region 
    /// <summary>
    /// Lấy một đối tượng từ Pool hoặc tạo mới nếu Pool hết.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: N/A
    /// </summary>
    /// <param name="position">Vị trí để đặt đối tượng</param>
    /// <param name="rotation">Góc xoay của đối tượng</param>
    /// <returns>Đối tượng được kích hoạt</returns>
    #endregion
    public GameObject GetObject(Vector3 position, Quaternion rotation)
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy) // Tìm đối tượng chưa hoạt động
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                return obj;
            }
        }

        // Nếu Pool hết, tạo thêm đối tượng mới
        GameObject newObj = CreateNewObject();
        newObj.transform.position = position;
        newObj.transform.rotation = rotation;
        newObj.SetActive(true);
        pool.Add(newObj);
        return newObj;
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