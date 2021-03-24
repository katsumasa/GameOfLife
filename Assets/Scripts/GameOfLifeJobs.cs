/*
 * Game of Life on C# Jobsystem
 *
 * C# Jobsystemをつかった ライフゲームのサンプルです。
 * Programed by Katsumasa Kimura
 * 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;


public class GameOfLifeJobs : MonoBehaviour {


    struct GameOfLifeJobParallelFor : IJobParallelFor {    
        // 表示面
        [ReadOnly] public NativeArray<int> srcs;
    
        // 描画面
        public NativeArray<int> dsts;

        // マップの横幅
        public int cellMapW;

        // マップの高さ
        public int cellMapH;


        //　<summary>
        //　Cell単位の実行処理
        // </summary>
        public void Execute(int id) {
            var y = id / cellMapW;
            var x = id - cellMapW * y;
            int val = 0;

            // (x,y)を中心とした周囲8cellを評価する
            for (var i = -1; i <= 1; i++) {
                var v = y + i;
                if (v < 0 || v >= cellMapH) {
                    continue;
                }

                for (var j = -1; j <= 1; j++) {
                    var u = x + j;
                    if (u < 0 || u >= cellMapW) {
                        continue;
                    } else if (x == u && y == v) {
                        continue;
                    }
                    var k = v * cellMapW + u;
                    val += srcs[k];
                }
            }

            dsts[id] = 0;
            //
            // LifeGame 23/3 の標準ルールを適用
            // 23/3 とは
            // 周囲に2つか3つの隣人がいれば生き残る
            // 周囲に3つの隣人がいれば生命が誕生し
            // 上記以外の場合では死亡
            //
            if (srcs[id] == 1)
            {
                if (val == 2 || val == 3)
                {
                    dsts[id] = 1;
                }
            } else if (val == 3) {
                dsts[id] = 1;
            }
        }
    }


    public int m_cellMapSize = 10;
    public float m_spacing = 2.0f;
    public GameObject m_model;
    public GameObject m_camera;
    GameOfLifeJobParallelFor m_lifeGameJob;
    JobHandle m_jobHandle;
    
    // Mesh
    Mesh mesh;

    // Material
    Material material;
    
    // MaterialPropertyBlock
    MaterialPropertyBlock m_materialPropertyBlock;
    
    // セルのマトリクス
    Matrix4x4[] m_matrix4X4s;
    
    // 生きているセルのリスト
    List<Matrix4x4> m_blueMatrixs;
    
    // 死亡しているセルのリスト
    List<Matrix4x4> m_blackMatrixs;



    // Use this for initialization
    void Start () {
        m_lifeGameJob = new GameOfLifeJobParallelFor();
        m_lifeGameJob.cellMapW = m_cellMapSize;
        m_lifeGameJob.cellMapH = m_cellMapSize;
        m_lifeGameJob.srcs = new NativeArray<int>(m_cellMapSize * m_cellMapSize,Allocator.Persistent);
        m_lifeGameJob.dsts = new NativeArray<int>(m_cellMapSize * m_cellMapSize,Allocator.Persistent);

        // 初期化
        for (var i = 0; i < m_lifeGameJob.srcs.Length; i++) {
            m_lifeGameJob.dsts[i] = Random.Range(0, 2);
        }

        m_materialPropertyBlock = new MaterialPropertyBlock();
        m_matrix4X4s = new Matrix4x4[m_cellMapSize * m_cellMapSize];
        m_blueMatrixs = new List<Matrix4x4>();
        m_blackMatrixs = new List<Matrix4x4>();

        var center = m_lifeGameJob.cellMapW * m_spacing * 0.5f;
        for (var h = 0; h < m_lifeGameJob.cellMapH; h++) {
            for(var w = 0; w < m_lifeGameJob.cellMapW; w++) {
                Vector3 position = new Vector3(w * m_spacing - center, h * m_spacing - center, 0);
                var idx = h * m_lifeGameJob.cellMapW + w;
                m_matrix4X4s[idx] = new Matrix4x4();
                m_matrix4X4s[idx].SetTRS(position, Quaternion.identity, Vector3.one);
            }
        }
        mesh = m_model.GetComponent<MeshFilter>().mesh;
        material = m_model.GetComponent<Renderer>().material;
    }


    // Update is called once per frame
    void Update () {
        // JOBの完了待ち
        m_jobHandle.Complete();

        // 結果を描画バッファにコピー
        // JOB SYSTEMのダブルバッファ処理のやり方が判らないので・・・
        m_lifeGameJob.dsts.CopyTo(m_lifeGameJob.srcs);

        // カメラの簡易ZOOM In/Out
        var v2 = Input.mouseScrollDelta;
        m_camera.transform.localPosition = new Vector3(
        m_camera.transform.localPosition.x,
        m_camera.transform.localPosition.y,
        m_camera.transform.localPosition.z + v2.y * 100.0f);

        // 描画処理
        // GPU Instanceは最大1024個迄しか処理出来ないので都度DrawKickする
        for (var i = 0; i < m_lifeGameJob.srcs.Length; i++) {
            if(m_lifeGameJob.srcs[i] == 0){
                m_blackMatrixs.Add(m_matrix4X4s[i]);
                if(m_blackMatrixs.Count >= 1023){
                    m_materialPropertyBlock.Clear();
                    m_materialPropertyBlock.SetColor("_Color", Color.black);
                    Graphics.DrawMeshInstanced(mesh, 0, material, m_blackMatrixs.ToArray(), m_blackMatrixs.Count, m_materialPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false);
                    m_blackMatrixs.Clear();
                }
            } else {
                m_blueMatrixs.Add(m_matrix4X4s[i]);
                if(m_blueMatrixs.Count >= 1023){
                    m_materialPropertyBlock.Clear();
                    m_materialPropertyBlock.SetColor("_Color", Color.green);
                    Graphics.DrawMeshInstanced(mesh, 0, material, m_blueMatrixs.ToArray(), m_blueMatrixs.Count, m_materialPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false);
                    m_blueMatrixs.Clear();
                }
            }
        }

        m_materialPropertyBlock.Clear();
        m_materialPropertyBlock.SetColor("_Color", Color.green);
        Graphics.DrawMeshInstanced(mesh,0, material,m_blueMatrixs.ToArray(),m_blueMatrixs.Count, m_materialPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false);
        m_blueMatrixs.Clear();

        m_materialPropertyBlock.Clear();
        m_materialPropertyBlock.SetColor("_Color", Color.black);
        Graphics.DrawMeshInstanced(mesh, 0, material, m_blackMatrixs.ToArray(), m_blackMatrixs.Count, m_materialPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false);
        m_blackMatrixs.Clear();

        // JOBの登録
        m_jobHandle = m_lifeGameJob.Schedule(m_cellMapSize * m_cellMapSize, 32);
    }


    void OnDestroy(){
        m_jobHandle.Complete();
        m_lifeGameJob.srcs.Dispose();
        m_lifeGameJob.dsts.Dispose();
    }
}
