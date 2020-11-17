class Session {

    constructor(){
        this.registeredNow = false;
    }
    getDate(date) {
        date = date ? date : new Date();
        return date.getDate().toString().padStart(2,'0') + '/' 
            + (date.getMonth() + 1).toString().padStart(2,'0') + '/' 
            + date.getFullYear();
    }

    groupBy(list, keyGetter) {
        const map = new Map();
        list.forEach((item) => {
            const key = keyGetter(item);
            const collection = map.get(key);
            if (!collection) {
                map.set(key, [item]);
            } else {
                collection.push(item);
            }
        });
        const result  = Array.from(map).map(data => { return { groupKey: data[0], values: data[1]  }; });;
        return result;
    }

    sort(array, keyGetter) {
        return array.sort((a,b)=>{
            let ka = keyGetter(a);
            let kb = keyGetter(b);
            ka = ka ? ka.toLowerCase() : '';
            kb = kb ? kb.toLowerCase() : '';

            return +(ka > kb) || +(ka === kb) - 1;
        });
    }

    formatIsoDateStr(text) {
        return text.replace(/^([0-9]{4})\-([0-9]{2})\-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9]{2}).*$/g, '$3/$2/$1 $4:$5');
    }

    convertToIsoStr(text) {
        return text.replace(/^([0-9]{2})\/([0-9]{2})\/([0-9]{4})$/g, '$3-$2-$1');
    }

    validateDateInput(text) {
        return text.replace(/^([0-9]{2})\/([0-9]{2})\/([0-9]{4})/g, '$3/$2/$1 00:00:00');
    }

    async login(accountId, password) {
        return await this.loginFetch('login', { 
            accountId: +accountId,
            password
        });
    }

    async createAccount(name, overdraftLimit, overdraftTax, password, openingDate, initialDeposit) {
        const result = await this.loginFetch('create', {
            name, overdraftLimit, overdraftTax, password, openingDate: this.convertToIsoStr(openingDate), initialDeposit
        });

        if (result.ok) {
            this.registeredNow = true;
        }

        return result;
    }

    async loginFetch(method, data) {
        const requestOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        };

        const response = await fetch('BankingAccount/' + method, requestOptions);
        let result = await this.handleResponse(response);

        if (result.ok) {
            localStorage.setItem('account_id', result.id);
            localStorage.setItem('name', result.name);
            localStorage.setItem('token', result.token);
        }

        return result;
    }

    async authFetch(method, data) {

        const token = localStorage.getItem('token');

        if (!token) {
            return {
                ok: false,
                error: 'auth',
                message: 'Faça login primeiro.'
            };
        }

        const requestOptions = {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token
            },
            body: JSON.stringify(data)
        };

        const response = await fetch('BankingAccount/' + method, requestOptions);
        console.log('HANDLE response: ' + JSON.stringify(response));
        let result = await this.handleResponse(response);
        return result;

    }

    async getBalace() {
        return await this.authFetch('balance', {});
    }

    async deposit(ammount) {
        return await this.authFetch('deposit', ammount);
    }

    async withdraw(ammount, password) {
        return await this.authFetch('withdraw', { ammount, password } );
    }

    async getStatement(dias) {
        let date = new Date();
        date.setDate(date.getDate()-dias);
        const dateStr = this.convertToIsoStr(this.getDate(date));
        return await this.authFetch('statement', dateStr );
    }

    async handleResponse(response) {
        console.log(response);
        if (response.ok) {
            return await response.json();
        } else {
            return {
                ok: false,
                error: response.status,
                message: response.statusText
            };
        }
    }

}

const session = new Session();
export default session;