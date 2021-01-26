## Sample data and a template have been provided to assist with testing.


### Flat file (CSV)

The following files can be used to test flat file (CSV) file parsing.  This is designed to illustrate a real world case where you might want to use a different delimiter than a comma.

| File        | Description           |
| ------------- |-------------|
| \SampleData\covid19-crvs-10-lines.csv | A sample 10 line flat file that uses pipes as a field delimiter. |
| \SampleData\covid19-crvs-10000-lines.csv | A sample 10000 line flat file that uses pipes as a field delimiter. Used to test larger payloads. |
| \Templates\cdc-covid19-crvs-to-hl7v251-vxuv04.liquid | Transform [CDC COVID-19 Vaccine Reporting Document (CRVS)](https://www.cdc.gov/vaccines/covid-19/reporting/requirements/specification-instructions.html) to [HL7 2.5.1 VXUV04](https://www.cdc.gov/vaccines/programs/iis/technical-guidance/hl7.html) |

  **[CDC COVID-19 Vaccine Reporting Document (CRVS)](https://www.cdc.gov/vaccines/covid-19/reporting/requirements/specification-instructions.html)** is a pipe field delimited flat file specification put out by the CDC to submit Patient and Immunization data related to COVID-19.
  
  **[HL7 2.5.1 VXUV04](https://www.cdc.gov/vaccines/programs/iis/technical-guidance/hl7.html)** is a slightly more complex flat file format used to pass patient and immunization between immunization registries and EHRS.


Transform a small pipe delimited file into a HL7 v2 file.  (Replace file and web paths with your own)
```shell
curl -i -H "Accept: text/plain" -H "Content-Type: text/csv" -H "Content-Type: text/plain" -H "Expect: 100-continue" -H "Content-Length: 3518" -H "X-CSV-Column-Delimiter: |"  --data-binary "@D:/source/repos/functions-dotnet-liquidtransform/data/SampleData/covid19-crvs-10-lines.csv"  -X POST "http://localhost:7071/api/liquidtransformer/cdc-covid19-crvs-to-hl7v251-vxuv04.liquid" -o myoutputhl7batchfilefrom10lines.txt
```  

Transform a large pipe delimited file into a HL7 v2 file.  (Replace file and web paths with your own)
```shell
curl -i -H "Accept: text/plain" -H "Content-Type: text/csv" -H "Content-Type: text/plain" -H "Expect: 100-continue" -H "Content-Length: 2860642" -H "X-CSV-Column-Delimiter: |"  --data-binary "@D:/source/repos/functions-dotnet-liquidtransform/data/SampleData/covid19-crvs-10000-lines.csv"  -X POST "http://localhost:7071/api/liquidtransformer/cdc-covid19-crvs-to-hl7v251-vxuv04.liquid" -o myoutputhl7batchfilefrom10000lines.txt
```  



