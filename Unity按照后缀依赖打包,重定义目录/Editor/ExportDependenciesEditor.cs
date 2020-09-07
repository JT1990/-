using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System; 
using System.Diagnostics;

public class ExportDependenciesEditor : EditorWindow
{
    static GameObject[] selectgos;
    static string rootFolderName = "Assets/checked";
    static string pFloderName;
    static string exportPath = "H:/UnityAssets.comb";
    static string exportFilename;
    static bool autoCloseWindow = false;
    


    /// <summary>
    /// 当前选择的物体的所有依赖文件路径
    /// </summary>
    static string[] m_dependencies;

    void Init()
    {

    }
    /// <summary>
    /// 快速打包
    /// </summary>
    //[MenuItem("Assets/ExportPackageQuickly _e", true, 1)]
    [MenuItem("Assets/ExportPackageQuickly _#E", false, 10)]
    public static void ExportPackageQuickly()
    {
        selectgos = Selection.gameObjects;
        pFloderName = selectgos[0].name;
        exportFilename = pFloderName;
        Export();

    }

    /// <summary>
    /// 更改到处路径和文件名称
    /// </summary> 
    [MenuItem("Assets/showExportWindow _#W", false, 1)]
    static void showWindow()
    {
        selectgos = Selection.gameObjects;
        pFloderName = selectgos[0].name;
        exportFilename = pFloderName;
        ExportDependenciesEditor mywindow = (ExportDependenciesEditor)EditorWindow.GetWindow(typeof(ExportDependenciesEditor), true, "ExportPackage setting", true);
    }

    private static void Export()
    {
        if (!AssetDatabase.IsValidFolder(rootFolderName))
        {
            AssetDatabase.CreateFolder(rootFolderName.Split('/')[0], rootFolderName.Split('/')[1]);
        }
        //在Assets下创建同名文件夹
        AssetDatabase.CreateFolder(rootFolderName, pFloderName);
        //找依赖关系
        for (int i = 0; i < selectgos.Length; i++)
        {

            m_dependencies = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(selectgos[i]));
            //根据依赖关系找文件 
            for (int j = 0; j < m_dependencies.Length; j++)
            {
                //UnityEngine.Debug.LogFormat("当前对象:{0} ,依赖关系:{1} -> {2}", selectgos[i].name, j, m_dependencies[j]);
                //如果是图片
                if (Path.GetExtension(m_dependencies[j]).ToLower() == ".psd" || Path.GetExtension(m_dependencies[j]).ToLower() == ".tiff" ||
                    Path.GetExtension(m_dependencies[j]).ToLower() == ".jpg" || Path.GetExtension(m_dependencies[j]).ToLower() == ".tga" ||
                    Path.GetExtension(m_dependencies[j]).ToLower() == ".png" || Path.GetExtension(m_dependencies[j]).ToLower() == ".gif" ||
                    Path.GetExtension(m_dependencies[j]).ToLower() == ".bmp" || Path.GetExtension(m_dependencies[j]).ToLower() == ".iff" || Path.GetExtension(m_dependencies[j]).ToLower() == ".pict")
                {
                    string filename = Path.GetFileName(m_dependencies[j]);
                    //没有文件夹创建
                    if (!AssetDatabase.IsValidFolder(rootFolderName + "/" + pFloderName + "/Texture"))
                        AssetDatabase.CreateFolder(rootFolderName + "/" + pFloderName, "Texture");

                    //已经在目标文件夹中存在,不移动
                    if (m_dependencies[j] != rootFolderName + "/" + pFloderName + "/Texture/" + filename)
                        AssetDatabase.MoveAsset(m_dependencies[j], rootFolderName + "/" + pFloderName + "/Texture/" + filename);
                }
                else
                {
                    string filename = Path.GetFileName(m_dependencies[j]);
                    string fileExtension = Path.GetExtension(m_dependencies[j]).TrimStart('.');

                    //没有文件夹创建
                    if (!AssetDatabase.IsValidFolder(rootFolderName + "/" + pFloderName + "/" + fileExtension))
                        AssetDatabase.CreateFolder(rootFolderName + "/" + pFloderName, fileExtension);

                    //已经在目标文件夹中存在,不移动
                    if (m_dependencies[j] != rootFolderName + "/" + pFloderName + "/" + fileExtension + "/" + filename)
                        AssetDatabase.MoveAsset(m_dependencies[j], rootFolderName + "/" + pFloderName + "/" + fileExtension + "/" + filename);
                }

            }
        }


        //导出 
        if (!Directory.Exists(exportPath.TrimEnd('/')))
        {
            Directory.CreateDirectory(exportPath.TrimEnd('/'));
        }

        CheckExportEixstFolder(exportFilename);
        AssetDatabase.ExportPackage(
            rootFolderName + "/" + pFloderName,
            exportPath + "/" + exportFilename + ".unitypackage",
            ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);


        Process.Start(Application.dataPath + "/Editor/Debug/Shot.exe", exportPath + "/" + exportFilename + ".png");

        UnityEngine.Debug.Log(exportPath + "/" + exportFilename + ".unitypackage");
        
    }

  

    private void OnGUI()
    {
        EditorGUILayout.LabelField("名称");
        exportFilename = EditorGUILayout.TextField(exportFilename);

        EditorGUILayout.LabelField("到处路径");
        exportPath = EditorGUILayout.TextField(exportPath);
        if (GUILayout.Button("更换文件夹"))
        {
            exportPath = EditorUtility.SaveFolderPanel("文件保存至",null,null);
        }
        if (GUILayout.Button("确定"))
        {
            Export();
            if (autoCloseWindow) this.Close();
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        autoCloseWindow=EditorGUILayout.Toggle("Auto Close Window", autoCloseWindow);
    }


    static void CheckAssetEixstFolder(string name, int flag = 0)
    {
        if (Directory.Exists(Application.dataPath + "/" + name))
        {
            CheckAssetEixstFolder(pFloderName + " " + (flag + 1).ToString(), flag + 1);
        }
        else
        {
            pFloderName = name;
        }
    }

    static void CheckExportEixstFolder(string name, int flag = 0)
    {
        if (File.Exists(exportPath + "/" + name + ".unitypackage"))
        {
            CheckExportEixstFolder(exportFilename + " " + (flag + 1).ToString(), flag + 1);
        }
        else
        {
            exportFilename = name;
        }
    }
}