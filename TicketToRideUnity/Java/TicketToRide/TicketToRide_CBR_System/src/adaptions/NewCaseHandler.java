package adaptions;

import java.util.Iterator;

import com.google.gson.JsonElement;
import com.google.gson.JsonObject;

import de.dfki.mycbr.core.Project;
import de.dfki.mycbr.core.casebase.Instance;
import model.Case;
import myCBR.Recommender;

public class NewCaseHandler extends Recommender<NewCaseRequest> {
	
	private Project project;
	
	public NewCaseHandler(Project project) {
		super(project);
		this.project = project;
	}
	
	public JsonElement process(NewCaseRequest context) {
		insertNewCase(context.newCase());
		var ans = new JsonObject();
		ans.addProperty("msg", "ok");
		return ans;
	}
	
	public void insertNewCase(Case newCase) {

		try {
			System.out.println("Groesse der Fallbasis (" + caseBase.getName() + "): " + caseBase.getCases().size());
			String nameOfNewCase = "";
		    nameOfNewCase = (caseBase.getCases().size() + 1) + "";
//		    nameOfNewCase = newCase.situation.playerHeroColor + (cb.getCases().size() + 1);
			
			Boolean nameOfCaseDone = false;
			int nameCounter = 2;
			String caseName = "";
			do {
				nameOfCaseDone = true;
				for (Iterator<Instance> iterator = caseBase.getCases().iterator(); iterator.hasNext();) {
					caseName = iterator.next().getName();
					if (nameOfNewCase.equals(caseName)) {
						nameOfCaseDone = false;
						nameOfNewCase = nameCounter + "";
//						nameOfNewCase = newCase.situation.playerHeroColor + nameCounter;
						nameCounter++;
					}
				}
			} while (!nameOfCaseDone);

			Instance caseToInsert = concept.addInstance(nameOfNewCase);

			fillInstanceWithSituationalKnowledge(caseToInsert, newCase.situation);

			caseToInsert.addAttribute(plan, plan.getAttribute(newCase.plan));
			caseBase.addCase(caseToInsert);

			System.out.println("Fall hinzugefuegt: " + nameOfNewCase);
			System.out.println("Groesse der Fallbasis (" + caseBase.getName() +  ") nach Hinzufuegen des Falls: " + caseBase.getCases().size());

//			project.getCaseBases().get("AllCases").addCase(caseToInsert);

			project.save();

		} catch (Exception exc) {
			System.out.println("Exception!");
			System.out.println(exc.getMessage());
		}
	}
}

