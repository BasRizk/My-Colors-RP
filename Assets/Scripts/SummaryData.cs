using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SummaryData {

	public int numOfDifferentVectors;
	public int numOfReversedVectors;
	public double vectorsDiffError;
	public double valuesDiffAbsError;
	public double valuesDiffRevError;
	public double valuesDiffSupError;
	public double valuesIdenticalPerVectorError;
	public double[] accumlativeErrors;

	public SummaryData() {
		numOfDifferentVectors = 4321;
		numOfReversedVectors = 123456;
		vectorsDiffError = 13.5;
		valuesDiffAbsError = 12;
		valuesDiffRevError = 10;
		valuesDiffSupError = 2.3333;
		valuesIdenticalPerVectorError = 15.6667;
		accumlativeErrors = new double[]{12,35,2,12,2,1};
	}
}
