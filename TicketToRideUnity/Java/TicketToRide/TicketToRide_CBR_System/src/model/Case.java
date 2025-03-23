package model;


public class Case {
	
	public String plan;
	public Situation situation;
	public String caseType;

	public Case() {

	}

	public Case(String plan, Situation situation, String caseType) {
        this.plan = plan;
        this.situation = situation;
        this.caseType = caseType;
	}
	
	public Case(String plan, String caseType) {
		this.plan = plan;
		this.caseType = caseType;
	}

}
