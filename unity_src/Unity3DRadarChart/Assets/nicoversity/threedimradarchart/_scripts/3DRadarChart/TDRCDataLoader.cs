using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Class responsible for loading the data (and other related sources, e.g., color coding) that is going to be visualized as a 3D Radar Chart.
/// </summary>
public class TDRCDataLoader : MonoBehaviour
{
    // private properties
    private ThreeDimRadarChartInterface tdrcInterface;                  // reference to overall interface
    private Dictionary<string, string> colorDict;                       // backup of original loaded color data
    private Dictionary<string, List<TDRCFrequencyPoint>> dataDict;      // backup of original loaded data


    #region PROPERTY_SETTERS

    /// <summary>
    /// Method to keep track of a reference to the 3D Radar Chart's main interface.
    /// </summary>
    /// <param name="tdrcInterfaceRef">Reference to the ThreeDimRadarChartInterface instance.</param>
    public void setTDRCInterface(ThreeDimRadarChartInterface tdrcInterfaceRef)
    {
        tdrcInterface = tdrcInterfaceRef;
    }

    #endregion


    #region DATA_LOADING

    /// <summary>
    /// Method to initaite the data loading routine in order to receive all necessary data to se tup the 3D Radar Chart.
    /// </summary>
    public void initiateDataLoading()
    {
        // Data loading documentation:
        // [1] Load and save data color configuration (this method).
        // [2] Load data and continue set up process (at the end of parseLoadedDataColors()).
        // [3] Initiate GameObject set up based on loaded data (at the end of parseLoadedData()).

        // [1] Load and save data color configuration.
        //

        if (tdrcInterface.isDataColorCodingLoadedFromServer) loadDataColorsFromServer(tdrcInterface.dataColorsURL);
        else loadDataColorsFromLocalDirectory(tdrcInterface.dataColorLocalFilePath);
    }

    /// <summary>
    /// Method to initiate data loading from server.
    /// </summary>
    /// <param name="url">String representing the URL to the data source.</param>
    private void loadDataFromServer(string url)
    {
        WWW wwwRequest = new WWW(url);
        StartCoroutine(WaitForDataFromServer(wwwRequest));
    }

    /// <summary>
    /// Method to handle server response for data loading process.
    /// </summary>
    /// <param name="wwwResponse">Server response.</param>
    /// <returns></returns>
    private IEnumerator WaitForDataFromServer(WWW wwwResponse)
    {
        yield return wwwResponse;   // wait until response completion from server

        if (wwwResponse.error != null)
        {
            // an error occured
            Debug.LogError("[TDRCDataLoader] WaitForDataFromServer " + wwwResponse.error);
            Debug.LogError("[TDRCDataLoader] WaitForDataFromServer Error URL = " + wwwResponse.url);
        }
        else
        {
            // data successfully received from server
            parseLoadedData(wwwResponse.text);
        }
    }

    /// <summary>
    /// Method to initiate data loading from local directory.
    /// </summary>
    /// <param name="filepath">String representing the filepath to the data source.</param>
    private void loadDataFromLocalDirectory(string filepath)
    {
        TextAsset csvContent = Resources.Load<TextAsset>(filepath);

        if (csvContent != null) parseLoadedData(csvContent.text);
        else Debug.LogError("[TDRCDataLoader] Unable to load data from local directory with filepath = " + filepath);
    }

    /// <summary>
    /// Method to initiate data color loading from server.
    /// </summary>
    /// <param name="url"></param>
    private void loadDataColorsFromServer(string url)
    {
        WWW wwwRequest = new WWW(url);
        StartCoroutine(WaitForDataColorsFromServer(wwwRequest));
    }

    /// <summary>
    /// Method to handle server response for data color loading process.
    /// </summary>
    /// <param name="wwwResponse">Server response.</param>
    /// <returns></returns>
    private IEnumerator WaitForDataColorsFromServer(WWW wwwResponse)
    {
        yield return wwwResponse;   // wait until response completion from server

        if (wwwResponse.error != null)
        {
            // an error occured
            Debug.LogError("[TDRCDataLoader] WaitForDataColorsFromServer " + wwwResponse.error);
            Debug.LogError("[TDRCDataLoader] WaitForDataColorsFromServer Error URL = " + wwwResponse.url);
        }
        else
        {
            // data successfully received from server
            parseLoadedDataColors(wwwResponse.text);
        }
    }

    /// <summary>
    /// Method to initiate data color loading from local directory.
    /// </summary>
    /// <param name="filepath"></param>
    private void loadDataColorsFromLocalDirectory(string filepath)
    {
        TextAsset csvContent = Resources.Load<TextAsset>(filepath);

        if (csvContent != null) parseLoadedDataColors(csvContent.text);
        else Debug.LogError("[TDRCDataLoader] Unable to load data colors from local directory with filepath = " + filepath);
    }

    #endregion


    #region DATA_PARSING

    /// <summary>
    /// Method to parse and process the loaded data color configuration.
    /// </summary>
    /// <param name="colorData">String representing the loaded color data.</param>
    private void parseLoadedDataColors(string colorData)
    {
        // Note: CSV model / template (= loaded color data) has two field names (string) "dimension", and (string) "color", and uses , as a delimiter
        // Note 2: The "color" string is coded in RGB hex color presentation, without the leading #, e.g., "ff00ff".

        // prepare text processing
        StringReader strReader = new StringReader(colorData);

        // create a dictionary with color (values) for each dimension (key)
        colorDict = new Dictionary<string, string>();
        while (strReader.Peek() > -1)
        {
            // read and dismantle current line
            string currentLine = strReader.ReadLine();
            string[] clParts = currentLine.Split(',');      // indexes: 0 = dimension, 1 = color

            // prepare key and value of current line
            string currentDimension = removeStringQuotes(clParts[0]);
            string currentColor = removeStringQuotes(clParts[1]);

            // ignore header row
            if (currentDimension.Equals("dimension") == false)
            {
                // add entry to color dictionary
                colorDict.Add(currentDimension, currentColor);
            }
        }

        // [2] Load data and continue set up process (follow up from initiateDataLoading()).
        //

        if (tdrcInterface.isDataLoadedFromServer) loadDataFromServer(tdrcInterface.dataSourceURL);
        else loadDataFromLocalDirectory(tdrcInterface.dataSourceLocalFilePath);
    }

    /// <summary>
    /// Method to parse and process the loaded data.
    /// </summary>
    /// <param name="dataText">String representing the loaded text data.</param>
    private void parseLoadedData(string textData)
    {
        // Note: CSV model / template (= loaded text data) has three field names (string) "dimension", (string) "time", and (int) value, and uses , as a delimiter

        // Method structure documentation:
        // [1] Parse the loaded text data to dynamically detect all existing unique dimension values.
        // [2] Create a dictionary with List<TDRCFrequencyPoint> (values) for each unique dimension (key).
        // [3] Parse through the loaded data again to populate individual list values (TDRCFrequencyPoint) for each dimension.


        // [1] Parse the loaded text data to dynamically detect all existing unique dimension values.
        //

        // prepare text processing
        StringReader strReader = new StringReader(textData);

        // dynamically detect individual dimensions in the loaded data (based on the "dimension" field)
        // by analyzing all rows in the data
        List<string> detectedDimensions = new List<string>();
        while (strReader.Peek() > -1)
        {
            // read and dismantle current line
            string currentLine = strReader.ReadLine();
            string[] clParts = currentLine.Split(',');      // indexes: 0 = dimension, 1 = time, 2 = value

            // get current dimension, and cut pre-/post-quote " of the string
            string currentDimension = removeStringQuotes(clParts[0]);

            // ignore header row
            if (currentDimension.Equals("dimension") == false)
            {
                // if current dimension is not yet in the captured key list, capture it
                if (detectedDimensions.Contains(currentDimension) != true) detectedDimensions.Add(currentDimension);
            }
        }

        // ATTENTION: Data visualzation as a 3D Radar Chart only makes sense if there are at least three data variables (= dimensions) in the data set (in order to create a radar mesh).
        // Developer Note: Consider deciding / implementing a maximum count for different data variables (= dimensions) in the future.
        if (detectedDimensions.Count < 3)
        {
            Debug.LogError("[TDRCDataLoader] parseLoadedData detected less than 3 (required) dimensions (= data variables), with textData = " + textData);
            return;     // abort further data processing and (GameObject) initialization
        }

        // [2] Create a dictionary with List<TDRCFrequencyPoint> (values) for each unique dimension (key).
        //

        // initialize lists of TDRCFrequencyPoints for individual dimensions (based on identified dimensions from parsed data)
        // and keep track of them in a dictionary (key = dimension, value = List of TDRCFrequencyPoints for that dimension)
        dataDict = new Dictionary<string, List<TDRCFrequencyPoint>>();
        for (int d = 0; d < detectedDimensions.Count; d++)
        {
            dataDict.Add(detectedDimensions[d], new List<TDRCFrequencyPoint>());
        }

        // [3] Parse through the loaded data again to populate individual list values (TDRCFrequencyPoint) for each dimension.
        //

        // populate data structure with content from loaded data
        strReader = new StringReader(textData);     // reset string reader for processing from the start
        while (strReader.Peek() > -1)
        {
            // read and dismantle current line
            string currentLine = strReader.ReadLine();
            string[] clParts = currentLine.Split(',');      // indexes: 0 = dimension, 1 = time, 2 = value

            // ignore header row
            if (removeStringQuotes(clParts[0]).Equals("dimension") == false)
            {
                // set up new TDRCFrequencyPoint based on current line
                TDRCFrequencyPoint fp = new TDRCFrequencyPoint();
                fp.dimension = removeStringQuotes(clParts[0]);
                fp.time = removeStringQuotes(clParts[1]);
                bool wasCurrentValueParsed = int.TryParse(clParts[2], out fp.value);
                if (wasCurrentValueParsed == false) Debug.LogError("[TDRCDataLoader] Error parsing int value for current line = " + currentLine);

                // determine to which dimension the current TDRCFrequencyPoint should be added
                foreach (string dimensionKey in dataDict.Keys)
                {
                    // check if current dimension key corresponds to the current TDRCFrequencyPoint's dimension
                    // if yes: add it to the List of TDRCFrequencyPoints, and break
                    if (dimensionKey.Equals(fp.dimension) == true)
                    {
                        dataDict[dimensionKey].Add(fp);
                        break;
                    }
                }
            }
        }


        // [3] Initiate GameObject set up based on loaded data (follow up from at the end of initiateDataLoading()).
        //

        tdrcInterface.freqPolyManager.initWithTDRCFrequencyPointListAndColorDictionary(dataDict, colorDict);
    }

    #endregion


    #region HELPER

    /// <summary>
    /// Method to cut pre- and post-quote " of the entered string value.
    /// </summary>
    /// <param name="text">String value with pre-/post-quote.</param>
    /// <returns>Entered string with removed quotes.</returns>
    private string removeStringQuotes(string text)
    {
        // check if entered string has a pre-quote (first char = "; following the assumption that if true there is also a post "):
        // if yes = remove pre-/post-quotes
        // if not = simply return entered text unchanged
        if (text[0].Equals('"')) return text.Substring(1, text.Length - 2);
        else return text;
    }

    #endregion
}
