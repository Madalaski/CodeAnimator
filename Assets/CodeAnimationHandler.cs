using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class CodeAnimationHandler : MonoBehaviour
{
    public enum Scope {
        LINE,
        CHARACTER
    }

    public enum Type {
        ADDITION,
        DELETION
    }

    [System.Serializable]
    public struct Change {

        public Change(Scope s, Type t, int ln, string txt, int cs, float wa) {
            scope = s;
            type = t;
            lineNo = ln;
            text = txt;
            charStart = cs;
            waitAddition = wa;
        }

        [SerializeField]
        public Scope scope;

        [SerializeField]
        public Type type;

        [SerializeField]
        public int lineNo;

        [TextArea]
        [SerializeField]
        public string text;

        [SerializeField]
        public int charStart;

        [SerializeField]
        public float waitAddition;
    }

    List<string> lines;

    [SerializeField]
    public List<Change> changes;

    //[TextArea]
    //public string checkText;

    //[SerializeField]
    //public List<Change> newChanges;

    public float typeSpeed = 0.1f;

    public TMP_Text code;

    public TMP_Text lineNumbers;

    public Camera cam;

    Vector3 targetCamPosition;

    Vector3 camVelocity;

    float lineHeight;

    Mesh textMesh;
    Vector3[] vertices;

    bool adding = false;
    int addStart = 0;
    int addLength = 0;
    float addTime = 0.0f;
    float addSpeed = 0.1f;

    public string fileName;

    // Start is called before the first frame update
    void Start()
    {
        lines = new List<string>();
        cam = Camera.main;
        targetCamPosition = cam.transform.position;
        string[] startLines = code.text.Split('\n');
        for (int i = 0; i < startLines.Length; i++) {
            lines.Add(startLines[i]);
        }
        code.ForceMeshUpdate();
        RePack();
        StartCoroutine(animate());
    }

    // Update is called once per frame
    void Update()
    {
        if (adding) {
            //UpdateMesh();
            //code.ForceMeshUpdate();
        }

        UpdateCamera();
    }

    void UpdateMesh() {
        code.ForceMeshUpdate();
        textMesh = code.mesh;
        vertices = textMesh.vertices;
        for (int j = 0; j < addLength; j++) {
            TMP_CharacterInfo c = code.textInfo.characterInfo[addStart + j];
            if (j == addLength - 1) {
                //print(c.character);
            }

            if (c.isVisible) {
                Vector3 centre = Vector3.zero;
                Vector3[] origins = new Vector3[4];

                for (int i = 0; i < 4; i++) {
                    origins[i] = vertices[c.vertexIndex + i];
                    centre += origins[i];
                }

                centre /= 4f;

                float factor = Mathf.Clamp01(((Time.time - addTime) - ((j+1)*typeSpeed)) / addSpeed);
                //Matrix4x4 rotate = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(Mathf.Lerp(180f, 360f, factor), Vector3.forward), Vector3.one * factor);
                Matrix4x4 mat = Matrix4x4.Scale(new Vector3(1f, factor, 1f));
                for (int i = 0; i < 4; i++) {
                    vertices[c.vertexIndex+i] = centre + (Vector3)(mat * (origins[i] - centre));
                }
            }
        }
        textMesh.vertices = vertices;
        code.canvasRenderer.SetMesh(textMesh);
    }

    void UpdateCamera() {
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetCamPosition, ref camVelocity, 0.2f);
    }

    void RePack() {
        string output = "";
        string current = "";
        for (int i = 0; i < lines.Count; i++) {
            //Debug.Log(i.ToString() + ":" + lines[i]);
            current = lines[i];
            current = HLSLFormat(current);
            output += current + "\n";
        }
        code.text = output;
    }

    void ChangeLine(int lineIndex) {
        int token = 0;

        for (int i = 0; i < lineIndex; i++) {
            token = code.text.IndexOf('\n', token) + 1;
        }
        
        int token2 = code.text.IndexOf('\n', token);

        //print(code.text.Substring(token, token2-token));

        string current = lines[lineIndex];

        current = HLSLFormat(current);

        //print(current);

        code.text = string.Join(string.Empty, code.text.Substring(0, token), current, code.text.Substring(token2));
    }

    string CSFormat(string current) {
        current = Format(current, "using", "#d33682");
        current = Format(current, "public", "#d33682");
        current = Format(current, "for", "#d33682");
        current = Format(current, "if", "#d33682");
        current = Format(current, "while", "#d33682");
        current = Format(current, "void", "#6c71c4");
        current = Format(current, "class", "#6c71c4");
        current = Format(current, "Vector", "#6c71c4", false, 1);
        current = Format(current, "int", "#6c71c4");
        current = Format(current, "//", "#859900", true);

        return current;
    }

    string HLSLFormat(string current) {
        current = Format(current, "Shader", "#d33682");
        current = Format(current, "Properties", "#d33682");
        current = Format(current, "SubShader", "#d33682");
        current = Format(current, "Pass", "#d33682");
        current = Format(current, "float", "#6c71c4", false, 1);
        current = Format(current, "fixed", "#6c71c4", false, 1);
        current = Format(current, "half", "#6c71c4", false, 1);
        current = Format(current, "sampler2D", "#6c71c4");
        current = Format(current, "#pragma", "#6c71c4");
        current = Format(current, "#include", "#6c71c4");
        current = Format(current, "//", "#859900", true);
        return current;
    }

    string Format(string input, string syntax, string color, bool wholeLine=false, int charsAfter = 0) {
        int index = input.IndexOf(syntax);
        if (index >= 0 && (wholeLine || ((charsAfter == 0 && index + syntax.Length < input.Length - 1 && input[index+syntax.Length] == ' ') || (charsAfter > 0 && index + syntax.Length + charsAfter <= input.Length - 1)) && (index == 0 || input[index-1] == ' ' || input[index-1] == '('))) {
            if (wholeLine) {
                return input.Substring(0, index) + "<color=" + color + ">" + input.Substring(index) + "</color>";
            }

            if (input[index + syntax.Length + charsAfter - 1] == ')') charsAfter--;

            return input.Substring(0, index) + "<color=" + color + ">" + syntax + input.Substring(index + syntax.Length, charsAfter) + "</color>" + input.Substring(index+syntax.Length+charsAfter);
        }
        return input;
    }

    IEnumerator animate() {
        yield return StartCoroutine(intro());
        foreach(Change c in changes) {
            yield return StartCoroutine(executeChange(c));
        }
    }

    IEnumerator intro() {
        code.ForceMeshUpdate();
        textMesh = code.mesh;
        vertices = textMesh.vertices;
        float startX = vertices[0].x;
        float startY = vertices[0].y;
        float width = vertices[4].x - startX;

        lineHeight = (code.textBounds.extents.y * 2f) / code.textInfo.lineCount * code.transform.lossyScale.y;

        //print(width);

        float startTime = Time.time;

        bool first = true;

        while (Time.time < startTime + 3.5f) {
            code.ForceMeshUpdate();
            textMesh = code.mesh;
            vertices = textMesh.vertices;
            for (int j = 0; j < code.textInfo.characterCount; j++) {
                TMP_CharacterInfo c = code.textInfo.characterInfo[j];
                if (c.isVisible) {
                    Vector3 centre = Vector3.zero;
                    Vector3[] origins = new Vector3[4];

                    for (int i = 0; i < 4; i++) {
                        origins[i] = vertices[c.vertexIndex + i];
                        centre += origins[i];
                    }
                    centre /= 4f;

                    float factor = Mathf.Clamp01(((Time.time - startTime) / typeSpeed) - (((centre.x - startX) + -(centre.y - startY)) / width));
                    


                    //print(factor);
                    //print(((Time.time - startTime) / typeSpeed) - ((centre.x - startX) / width));

                    //Matrix4x4 mat = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(Mathf.Lerp(270f, 360f, factor), Vector3.forward), new Vector3(1f, factor, 1f));
                    Matrix4x4 mat = Matrix4x4.Scale(new Vector3(1f, factor, 1f));
                    for (int i = 0; i < 4; i++) {
                        vertices[c.vertexIndex+i] = centre + (Vector3)(mat * (origins[i] - centre));
                    }

                }
            }

            textMesh.vertices = vertices;
            code.canvasRenderer.SetMesh(textMesh);
            if (first) {
                yield return new WaitForSeconds(1f);
                first = false;
                startTime = Time.time;
            }
            yield return null;
        }
    }

    IEnumerator executeChange(Change c) {
        string line = lines[c.lineNo-1];

        if (Mathf.Abs(cam.transform.position.y - (c.lineNo * -lineHeight)) > (cam.orthographicSize / 6f)) {
            targetCamPosition.y = Mathf.Min((c.lineNo * -lineHeight), -cam.orthographicSize);
        } 

        switch (c.scope) {
            case Scope.LINE:
                switch (c.type) {
                    case Type.DELETION:
                        for (int i = line.Length - 1; i >= 0; i--) {
                            lines[c.lineNo-1] = line.Substring(0, i);
                            ChangeLine(c.lineNo-1);
                            yield return new WaitForSeconds(typeSpeed);
                        }
                        lines.RemoveAt(c.lineNo-1);
                        break;
                    case Type.ADDITION:
                        string[] newLines = c.text.Split('\n');
                        for (int i = 0; i < newLines.Length; i++) {
                            lines.Insert(c.lineNo+i, "");
                            RePack();
                            adding = true;
                            addStart = code.textInfo.lineInfo[c.lineNo+i].firstCharacterIndex;
                            addLength = -1;
                            addTime = Time.time;
                            while (Time.time < addTime + (newLines[i].Length+1)*typeSpeed) {
                                int currentLength = Mathf.FloorToInt((Time.time - addTime) / typeSpeed);
                                if (addLength < currentLength - 1) {
                                    addLength = currentLength - 1;
                                    lines[c.lineNo+i] = newLines[i].Substring(0, addLength);
                                    ChangeLine(c.lineNo+i);
                                    UpdateMesh();
                                }
                                yield return null;
                            }

                            addLength = newLines[i].Length+1;

                            lines[c.lineNo+i] = newLines[i];
                        }
                        
                        /*for (int i = 0; i <= c.text.Length; i++) {
                            lines[c.lineNo] = c.text.Substring(0, i);
                            RePack();
                            addLength++;
                            //StartCoroutine(spinCharacter(c.lineNo, i-1, typeSpeed*8f));
                            yield return new WaitForSeconds(typeSpeed);
                        }*/
                        break;
                }
                break;
            case Scope.CHARACTER:
                switch (c.type) {
                    case Type.DELETION:
                        int startIndex = line.IndexOf(c.text);
                        int endIndex = startIndex + c.text.Length;
                        for (int i = c.text.Length - 1; i >= 0; i--) {
                            lines[c.lineNo-1] = line.Substring(0, startIndex) + line.Substring(startIndex, i) + line.Substring(endIndex);
                            ChangeLine(c.lineNo-1);
                            yield return new WaitForSeconds(typeSpeed);
                        }
                        break;
                    case Type.ADDITION:
                        int charIndex = c.charStart;
                        adding = true;
                        //print(code.textInfo.lineInfo[c.lineNo-1].characterCount);
                        addStart = code.textInfo.lineInfo[c.lineNo-1].firstCharacterIndex + charIndex;
                        addLength = 0;
                        addTime = Time.time;
                        while (Time.time < addTime + (c.text.Length+1)*typeSpeed) {
                            int currentLength = Mathf.FloorToInt((Time.time - addTime) / typeSpeed);
                            if (addLength < currentLength - 1) {
                                addLength = currentLength - 1;
                                
                                lines[c.lineNo-1] = line.Substring(0, charIndex) + c.text.Substring(0, addLength) + line.Substring(charIndex);
                                ChangeLine(c.lineNo-1);
                                UpdateMesh();
                            }
                            yield return null;
                        }

                        lines[c.lineNo-1] = line.Substring(0, charIndex) + c.text + line.Substring(charIndex);
                        addLength = c.text.Length + 1;

                        /*for (int i = 0; i <= c.text.Length; i++) {
                            lines[c.lineNo-1] = line.Substring(0, charIndex) + c.text.Substring(0, i) + line.Substring(charIndex);
                            RePack();
                            addLength++;
                            //StartCoroutine(spinCharacter(c.lineNo-1, charIndex+i-1, typeSpeed*8f));
                            yield return new WaitForSeconds(typeSpeed);
                        }*/
                        break;
                }
                break;
        }

        //Debug.Log("FINISHED CHANGE");
        //adding = false;
        RePack();
        yield return new WaitForSeconds((typeSpeed * 5f) + c.waitAddition);
    }

    IEnumerator spinCharacter(int lIndex, int cIndex, float speed) {
        //code.ForceMeshUpdate();

        vertices = code.mesh.vertices;

        int firstIndex = code.textInfo.lineInfo[lIndex].firstCharacterIndex;

        TMP_CharacterInfo c = code.textInfo.characterInfo[firstIndex + cIndex];

        if (!c.isVisible) yield break;

        print(c.vertexIndex);

        float startTime = Time.time;

        Vector3 centre = Vector3.zero;
        Vector3[] origins = new Vector3[4];

        for (int i = 0; i < 4; i++) {
            origins[i] = vertices[c.vertexIndex + i];
            centre += origins[i];
        }

        centre /= 4f;

        Mesh mesh;

        while (Time.time < startTime + speed) {
            mesh = code.mesh;
            vertices = mesh.vertices;
            float factor = (Time.time - startTime) / speed;
            //Matrix4x4 rotate = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(Mathf.Lerp(180f, 360f, factor), Vector3.forward), Vector3.one * factor);
            Matrix4x4 mat = Matrix4x4.Scale(new Vector3(1f, factor, 1f));
            for (int i = 0; i < 4; i++) {
                vertices[c.vertexIndex+i] = centre + (Vector3)(mat * (origins[i] - centre));
            }
            mesh.vertices = vertices;
            code.canvasRenderer.SetMesh(mesh);
            yield return null;
        }

        mesh = code.mesh;
        for (int i = 0; i < 4; i++) {
            vertices[c.vertexIndex+i] =  origins[i];
        }
        mesh.vertices = vertices;
        code.canvasRenderer.SetMesh(mesh);
        //code.ForceMeshUpdate();

        yield return null;
    }
    
    public void ReadFromFile() {
        StreamReader sr = new StreamReader(Application.dataPath + "/" + fileName);
        string file = sr.ReadToEnd();
        sr.Close();
        string[] fileLines = file.Split('\n');
        List<string> startLines = new List<string>();
        List<Change> changeLines = new List<Change>();

        int prevChangeNo = -1;

        string lineNoText = "";

        for (int i = 0; i < fileLines.Length; i++) {
            //print(fileLines[i]);

            lineNoText += (i+1).ToString() + "\n";

            if (fileLines[i].Length > 3) {
                if (fileLines[i].Substring(0,3) == "+++") {
                    int spaceLoc = fileLines[i].IndexOf(';');
                    int changeNo = int.Parse(fileLines[i].Substring(3, spaceLoc - 3));

                    if (changeNo < prevChangeNo) {
                        startLines.Add("%");
                    }

                    for (int j = changeLines.Count; j <= changeNo; j++) {
                        changeLines.Add(new Change());
                    }

                    changeLines[changeNo] = new Change(Scope.LINE, Type.ADDITION,
                        startLines.Count, fileLines[i].Substring(spaceLoc+1), 0, 0f);
                    
                    prevChangeNo = changeNo;
                }
                else if (fileLines[i].Substring(0,3) == "---") {
                    int spaceLoc = fileLines[i].IndexOf(';');
                    int changeNo = int.Parse(fileLines[i].Substring(3, spaceLoc - 3));

                    if (changeNo < prevChangeNo) {
                        startLines.Add("%");
                    }

                    for (int j = changeLines.Count; j <= changeNo; j++) {
                        changeLines.Add(new Change());
                    }

                    startLines.Add(fileLines[i].Substring(spaceLoc+1));

                    changeLines[changeNo] = new Change(Scope.LINE, Type.DELETION,
                        startLines.Count, "", 0, 0f);
                    prevChangeNo = changeNo;
                }
                else if (fileLines[i].Substring(0,3) == "~~~") {
                    int spaceLoc = fileLines[i].IndexOf(';');
                    int changeNo = int.Parse(fileLines[i].Substring(3, spaceLoc - 3));

                    if (changeNo < prevChangeNo) {
                        startLines.Add("%");
                    }

                    for (int j = changeLines.Count; j <= changeNo+1; j++) {
                        changeLines.Add(new Change());
                    }

                    string newLine = fileLines[i].Substring(spaceLoc+1);
                    string oldLine = fileLines[i-1];
                    if (!startLines.Contains(oldLine)) {
                        oldLine = oldLine.Substring(oldLine.IndexOf(';')+1);
                    }

                    int first = 0;

                    while (first < Mathf.Min(newLine.Length, oldLine.Length) && oldLine[first] == newLine[first]) {
                        first++;
                    }

                    int last = 0;

                    while (last < Mathf.Min(newLine.Length, oldLine.Length)
                        && oldLine[oldLine.Length - 1 - last] == newLine[newLine.Length - 1 - last]) {
                        last++;
                    }

                    changeLines[changeNo] = new Change(Scope.CHARACTER, Type.DELETION,
                        startLines.Count, oldLine.Substring(first, Mathf.Max(0, oldLine.Length - (last + first))), 0, 0f);
                    changeLines[changeNo + 1] = new Change(Scope.CHARACTER, Type.ADDITION,
                        startLines.Count, newLine.Substring(first, Mathf.Max(0, newLine.Length - (last + first))), first, 0f);
                    prevChangeNo = changeNo;
                }
                else {
                    startLines.Add(fileLines[i]);
                    prevChangeNo = -1;
                }
            }
            else {
                startLines.Add(fileLines[i]);
                prevChangeNo = -1;
            }
        }

        lineNumbers.text = lineNoText;

        int[] lineOffsets = new int[startLines.Count];

        int shift = 0;

        for (int i = 0; i < lineOffsets.Length; i++) {
            if (startLines[i].Length < 3 && startLines[i].Contains("%")) shift++;
            lineOffsets[i] = -shift;
        }

        for (int i = 0; i < changeLines.Count; i++) {
            //print(i);
            Change buffer = changeLines[i];
            buffer.lineNo += lineOffsets[buffer.lineNo-1];
            if (buffer.scope == Scope.LINE) {
                int addition = (buffer.type == Type.ADDITION) ? 1 : -1;
                //int index = (buffer.type == Type.ADDITION) ? -1 : 0;
                for (int j = changeLines[i].lineNo - 1; j < lineOffsets.Length; j++) {
                    lineOffsets[j] += addition;
                }
                /*string offsetString = "";
                for (int j = 0;j < lineOffsets.Length; j++) {
                    offsetString += lineOffsets[j].ToString() + ", ";
                }
                print(offsetString);*/
            }
            changeLines[i] = buffer;
        }

        

        string output = "";
        string current = "";
        for (int i = 0; i < startLines.Count; i++) {
            current = startLines[i];
            if (!(startLines[i].Length < 3 && startLines[i].Contains("%"))) {
                output += current + "\n";
            }
        }
        code.text = output;

        changes = changeLines;



    }

}