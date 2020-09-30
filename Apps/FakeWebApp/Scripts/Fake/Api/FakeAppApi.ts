// Generated code

import { AppApi } from "../../Hub/AppApi";
import { AppApiEvents } from "../../Hub/AppApiEvents";
import { EmployeeGroup } from "./EmployeeGroup";
import { ProductGroup } from "./ProductGroup";

export class FakeAppApi extends AppApi {
	constructor(events: AppApiEvents, baseUrl: string, version: string = 'V7') {
		super(events, baseUrl, 'Fake', version);
		this.Employee = this.addGroup((evts, resourceUrl) => new EmployeeGroup(evts, resourceUrl));
		this.Product = this.addGroup((evts, resourceUrl) => new ProductGroup(evts, resourceUrl));
	}

	readonly Employee: EmployeeGroup;
	readonly Product: ProductGroup;
}