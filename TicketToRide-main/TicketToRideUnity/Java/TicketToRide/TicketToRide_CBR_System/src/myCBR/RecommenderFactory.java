package myCBR;

import adaptions.NewCaseHandler;
import adaptions.SolutionAdapter;
import de.dfki.mycbr.core.Project;
import model.Request;
import model.Request.RequestType;
import retrieval.RetrievalHandler;

public class RecommenderFactory {
	public static Recommender<? extends Request> create(Project project, RequestType type) {
		return switch (type) {
		case RETRIEVAL -> new RetrievalHandler(project);
		case ADAPTION -> new SolutionAdapter(project);
		case NEWCASE -> new NewCaseHandler(project);
		default -> throw new IllegalArgumentException("" + type);
		};
	}
}
