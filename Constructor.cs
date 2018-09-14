using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class Save : EditorWindow
{
    private static string _robotName;
    private static string _fileName;
    private static Save saveWindow;

    [MenuItem("Constructor/Save Objects %#E")]
    static void OpenSaveWindow()
    {
        saveWindow = (Save)EditorWindow.GetWindow(typeof(Save));

        saveWindow.Show();
    }

    static void SaveToFile()
    {
        StringBuilder sb = new StringBuilder();
        StringWriter stringWriter = new StringWriter(sb);
        GenerateHeader(sb);
        var links = FindObjectsOfType(typeof(Link));
        for (int i = 0; i < links.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Saving links", string.Concat(i + 1, "/", links.Length), (i + 1) / (float)links.Length);
            GenerateLink(sb, links[i] as Link);
        }
        var joints = FindObjectsOfType(typeof(Joint));
        for (int i = 0; i < joints.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Saving joints", string.Concat(i + 1, "/", joints.Length), (i + 1) / (float)joints.Length);
            GenerateJoint(sb, joints[i] as Joint);
        }
        GenerateFooter(sb);
        stringWriter.Flush();
        stringWriter.Dispose();
        if (!Directory.Exists(string.Concat(Application.dataPath, "/", "URDFFiles")))
            Directory.CreateDirectory(string.Concat(Application.dataPath, "/", "URDFFiles"));
        File.WriteAllText(string.Concat(Application.dataPath, "/URDFFiles/", _fileName, ".urdf"), sb.ToString());
        EditorUtility.ClearProgressBar();
    }

    private static void GenerateHeader(StringBuilder sb)
    {
        sb.AppendLine("<?xml version=\"1.0\"?>");
        sb.AppendLine(string.Concat("<robot name=\"", _robotName,"\" xmlns:xacro=\"http://www.ros.org/wiki/xacro \">"));
    }

    private static void GenerateFooter(StringBuilder sb)
    {
        sb.AppendLine("</robot>");
    }

    private static void GenerateLink(StringBuilder sb, Link link)
    {
        if (link == null)
            return;
        sb.AppendLine(string.Concat("<link name=", link.name, ">"));
        sb.AppendLine("      <visual>");
        sb.AppendLine("        <geometry>");
        sb.AppendLine(string.Concat("          <mesh filename = \"../meshes/", link.name, ".dae\" scale = \"", link.transform.localScale.x, " ", link.transform.localScale.y, " ", link.transform.localScale.z, "\" />"));
        sb.AppendLine("        </geometry>");
        sb.AppendLine("      </visual>");
        sb.AppendLine("      <collision>");
        sb.AppendLine("        <geometry>");
        sb.AppendLine(string.Concat("          <mesh filename = \"../meshes/", link.name, ".dae\" scale = \"", link.transform.localScale.x, " ", link.transform.localScale.y, " ", link.transform.localScale.z, "\" />"));
        sb.AppendLine("        </geometry>");
        sb.AppendLine("      </collision>");
        sb.AppendLine("    <inertial>");
        sb.AppendLine(string.Concat("      <mass value=\"", link.mass, "\" />"));
        sb.AppendLine(string.Concat("      <inertia ixx=\"", link.inertialX.x,
                                                "\" ixy=\"", link.inertialX.y,
                                                "\" ixz=\"", link.inertialX.z,
                                                "\" iyx=\"", link.inertialY.x,
                                                "\" iyy=\"", link.inertialY.y,
                                                "\" iyz=\"", link.inertialY.z,
                                                "\" izx=\"", link.inertialZ.x,
                                                "\" izy=\"", link.inertialZ.y,
                                                "\" izz=\"", link.inertialZ.z,
                                                "\" />"));
        sb.AppendLine("    </inertial>");
        sb.AppendLine("    </link>");
    }

    private static void GenerateJoint(StringBuilder sb, Joint joint)
    {
        if (joint == null)
            return;
        sb.AppendLine(string.Concat("    <joint name=\"", joint.name, "\" type=\"", joint.type.ToString().ToLower(), "\">"));
        sb.AppendLine(string.Concat("      <parent link=\"", joint.transform.parent.name, "\"/>"));
        sb.AppendLine(string.Concat("      <child link=\"", joint.name, "\"/>"));
        sb.AppendLine(string.Concat("      <origin xyz=\"", joint.transform.localPosition.x, " ", joint.transform.localPosition.y, " ", joint.transform.localPosition.z,
                                               "\" rpy=\"", joint.transform.localEulerAngles.x, " ", joint.transform.localEulerAngles.y, " ", joint.transform.localEulerAngles.z, "\"/>"));
        if (joint.mimic != null)
            sb.AppendLine(string.Concat("      <mimic joint=\"", joint.mimic.name, "\"/>"));
        sb.AppendLine("    </joint>");
    }

    void OnGUI()
    {

        _robotName = EditorGUI.TextField(new Rect(3, 3, position.width - 6, 16), "Robot Name", _robotName);
        _fileName = EditorGUI.TextField(new Rect(3, 25, position.width - 6, 16), "File Name", _fileName);

        if (string.IsNullOrEmpty(_robotName) || string.IsNullOrEmpty(_fileName))
            EditorGUI.LabelField(new Rect(3, 52, position.width, 20), "Please enter the names");
        else
        {

            if (GUI.Button(new Rect(3, 52, position.width - 6, 20), "Save"))
            {
                SaveToFile();
            }
        }
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
}

public class Create : EditorWindow
{
    private int _objects_count = 1;
    private Transform _parent;
    private GameObject _object;
    private GameObject _tempObject;

    private int _selected;
    private static Vector3 _scale;
    private static Vector3 _position;
    private static Vector3 _rotation;

    [MenuItem("Constructor/Create Object %#R")]
    static void InitMultiCreateWindow()
    {

        Create window = (Create)EditorWindow.GetWindow(typeof(Create));

        window.Show();
    }



    void OnGUI()
    {
        _object = EditorGUI.ObjectField(new Rect(3, 3, position.width - 6, 16), "Object", _object, typeof(GameObject), true) as GameObject;
        _scale = EditorGUI.Vector3Field(new Rect(3, 25, position.width - 6, 16), "Scale", _scale);
        _position = EditorGUI.Vector3Field(new Rect(3, 60, position.width - 6, 16), "Position", _position);
        _rotation = EditorGUI.Vector3Field(new Rect(3, 95, position.width - 6, 16), "Rotation", _rotation);



        if (_object == null)
            EditorGUI.LabelField(new Rect(3, 140, position.width, 20), "Please choose on a GameObject for create!");

        else
        {

            if (GUI.Button(new Rect(3, 140, position.width - 6, 20), "Create object"))
            {
                if (_object != null)
                {
                    if (Selection.activeGameObject != null)
                    {
                        _tempObject = Instantiate(_object, _position, Quaternion.identity, Selection.activeGameObject.transform);
                        _tempObject.transform.localScale = _scale;
                        _tempObject.transform.localEulerAngles = _rotation;
                        _tempObject.AddComponent<Joint>();
                        _tempObject.AddComponent<Link>();


                    }
                    else
                    {
                        _tempObject = Instantiate(_object, _position, Quaternion.identity);
                        _tempObject.transform.localScale = _scale;
                        _tempObject.transform.localEulerAngles = _rotation;
                        _tempObject.AddComponent<Link>();
                    }
                    string tempName = _tempObject.name.Replace("(Clone)", "");

                    if (GameObject.Find(tempName))
                    {
                        int i = 1;
                        while (GameObject.Find(string.Concat(tempName, i)) != null)
                        {
                            i++;
                        }
                        tempName = string.Concat(tempName, i);
                    }
                    _tempObject.name = tempName;
                }
            }
        }

    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
}