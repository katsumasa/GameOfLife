using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game of Life�̃m�[�}��C#�o�[�W����
/// Programed by Katsumasa Kimura
/// </summary>
public class GameOfLifeCS : MonoBehaviour
{
    // �}�b�v�̉���
    public int cellMapW;

    // �}�b�v�̍���
    public int cellMapH;

    public float m_spacing = 2.0f;

    public GameObject m_model;

    public GameObject m_camera;

    // �\���p
    int[] srcs;

    // �v�Z�p
    int[] dsts;

    // Mesh
    Mesh mesh;

    // Material
    Material material;

    // MaterialPropertyBlock
    MaterialPropertyBlock m_materialPropertyBlock;

    // �Z���̃}�g���N�X
    Matrix4x4[] m_matrix4X4s;

    // �����Ă���Z���̃��X�g
    List<Matrix4x4> m_blueMatrixs;

    // ���S���Ă���Z���̃��X�g
    List<Matrix4x4> m_blackMatrixs;


    public void Execute(int id)
    {
        var y = id / cellMapW;
        var x = id - cellMapW * y;
        int val = 0;

        // (x,y)�𒆐S�Ƃ�������8cell��]������
        for (var i = -1; i <= 1; i++)
        {
            var v = y + i;
            if (v < 0 || v >= cellMapH)
            {
                continue;
            }

            for (var j = -1; j <= 1; j++)
            {
                var u = x + j;
                if (u < 0 || u >= cellMapW)
                {
                    continue;
                }
                else if (x == u && y == v)
                {
                    continue;
                }
                var k = v * cellMapW + u;
                val += srcs[k];
            }
        }

        dsts[id] = 0;
        //
        // LifeGame 23/3 �̕W�����[����K�p
        // 23/3 �Ƃ�
        // ���͂�2��3�̗אl������ΐ����c��
        // ���͂�3�̗אl������ΐ������a����
        // ��L�ȊO�̏ꍇ�ł͎��S
        //
        if (srcs[id] == 1)
        {
            if (val == 2 || val == 3)
            {
                dsts[id] = 1;
            }
        }
        else if (val == 3)
        {
            dsts[id] = 1;
        }
    }



// Start is called before the first frame update
void Start()
    {
        srcs = new int[cellMapH * cellMapW];
        dsts = new int[cellMapH * cellMapW];

        for(var i = 0; i < srcs.Length; i++)
        {
            srcs[i] = Random.RandomRange(0, 2);
        }

        m_materialPropertyBlock = new MaterialPropertyBlock();
        m_matrix4X4s = new Matrix4x4[cellMapH * cellMapW];
        m_blueMatrixs = new List<Matrix4x4>();
        m_blackMatrixs = new List<Matrix4x4>();

        var center = cellMapW * m_spacing * 0.5f;
        for (var h = 0; h < cellMapH; h++)
        {
            for (var w = 0; w < cellMapW; w++)
            {
                Vector3 position = new Vector3(w * m_spacing - center, h * m_spacing - center, 0);
                var idx = h * cellMapW + w;
                m_matrix4X4s[idx] = new Matrix4x4();
                m_matrix4X4s[idx].SetTRS(position, Quaternion.identity, Vector3.one);
            }
        }
        mesh = m_model.GetComponent<MeshFilter>().mesh;
        material = m_model.GetComponent<Renderer>().material;
    }


    // Update is called once per frame
    void Update()
    {
        
        // �J�����̊Ȉ�ZOOM In/Out
        var v2 = Input.mouseScrollDelta;
        m_camera.transform.localPosition = new Vector3(
        m_camera.transform.localPosition.x,
        m_camera.transform.localPosition.y,
        m_camera.transform.localPosition.z + v2.y * 100.0f);

        // �`�揈��
        // GPU Instance�͍ő�1024�����������o���Ȃ��̂œs�xDrawKick����
        for (var i = 0; i < srcs.Length; i++)
        {
            if (srcs[i] == 0)
            {
                m_blackMatrixs.Add(m_matrix4X4s[i]);
                if (m_blackMatrixs.Count >= 1023)
                {
                    m_materialPropertyBlock.Clear();
                    m_materialPropertyBlock.SetColor("_Color", Color.black);
                    Graphics.DrawMeshInstanced(mesh, 0, material, m_blackMatrixs.ToArray(), m_blackMatrixs.Count, m_materialPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false);
                    m_blackMatrixs.Clear();
                }
            }
            else
            {
                m_blueMatrixs.Add(m_matrix4X4s[i]);
                if (m_blueMatrixs.Count >= 1023)
                {
                    m_materialPropertyBlock.Clear();
                    m_materialPropertyBlock.SetColor("_Color", Color.green);
                    Graphics.DrawMeshInstanced(mesh, 0, material, m_blueMatrixs.ToArray(), m_blueMatrixs.Count, m_materialPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false);
                    m_blueMatrixs.Clear();
                }
            }
        }

        m_materialPropertyBlock.Clear();
        m_materialPropertyBlock.SetColor("_Color", Color.green);
        Graphics.DrawMeshInstanced(mesh, 0, material, m_blueMatrixs.ToArray(), m_blueMatrixs.Count, m_materialPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false);
        m_blueMatrixs.Clear();

        m_materialPropertyBlock.Clear();
        m_materialPropertyBlock.SetColor("_Color", Color.black);
        Graphics.DrawMeshInstanced(mesh, 0, material, m_blackMatrixs.ToArray(), m_blackMatrixs.Count, m_materialPropertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false);
        m_blackMatrixs.Clear();


        for (var i = 0; i < srcs.Length; i++)
        {
            Execute(i);
        }

        // �_�u���o�b�t�@�ɂ���΁A�R�s�[�̕K�v���������AJobsystem���ɏ��������킹��ׁA�V���O���o�b�t�@�ŏ���
        for (var i = 0; i < srcs.Length; i++)
        {
            srcs[i] = dsts[i];
        }

    }
}
