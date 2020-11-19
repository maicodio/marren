import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import { Button, FormFeedback, Input, InputGroup, InputGroupAddon, InputGroupText } from 'reactstrap';
import session from './Session';

export class Login extends Component {
  static displayName = Login.name;

  constructor(props) {
    super(props);
    this.pwdRef = React.createRef();
    this.state = {
      login: { accountId: localStorage.getItem('account_id'), password: '', errors: null },
      cancelado: false,
      loading: false,
      logado: false,
      trigger: false,
    };
  }

  changeLoginAccountId(value) {
    const login = this.state.login;
    login.accountId = value;
    this.setState({ login: login });
  }

  changeLoginPassword(value) {
    const login = this.state.login;
    login.password = value;
    this.setState({ login: login });
  }

  async handleKeyPressPassword(event) {
    if(event.key === 'Enter'){
      await this.login();
    }
  }

  async handleKeyPressAccountId(event) {
    if (event.key === 'Enter') {
      ReactDOM.findDOMNode(this.pwdRef.current).focus();
    }
  }

  async login() {
    const login = this.state.login;
    login.errors = null;
    this.setState({login, loading: true});
    const result = await session.login(login.accountId, login.password);
    
    if (!result.ok) {
      const errors = {};
      if (result.validationErrors && result.validationErrors.length) {
        result.validationErrors.forEach(x=>{
          errors[x.id] = x.message;
        });
        login.errors = errors;
        this.setState({login, loading: false});
        return;
      }
      errors.Password = result.message;
      login.errors = errors;
      this.setState({login, loading: false});
      return;
    }
    this.setState({logado: true, loading: false, trigger: true},() => {
      this.props.triggerNextStep({ trigger: 'login-welcome' });
    });
  }

  cancel() {
    this.setState({cancelado: true, logado: false, loading: false, trigger: true}, () => {
      this.props.triggerNextStep({ trigger: 'welcome' });
    });
  }

  render () {

    if (this.state.loading) {
      return (<div>Carregando...</div>)   
    }

    if (this.state.logado) {
      return (<div>Login realizado com sucesso!</div>)   
    }

    if (this.state.cancelado) {
      return (<div>Login cancelado.</div>)   
    }

    return (
      <div className="login">
        Vamos fazer o login.
        Informe o número da conta:
        <InputGroup>
          <Input
            type="number"
            invalid={!!this.state.login.errors?.AccountId}
            value={this.state.login.accountId ?? ''}
            onFocus={e => e.target.select()}
            placeholder="Número da Conta"
            onChange={e => this.changeLoginAccountId(e.target.value)}
            onKeyPress={e => this.handleKeyPressAccountId(e)} />
          {this.state.login.errors?.AccountId ? <FormFeedback>{this.state.login.errors?.AccountId}</FormFeedback> : null}
        </InputGroup>
        Informe a senha:
        <InputGroup>
          <Input
            type="password"
            invalid={!!this.state.login.errors?.Password}
            value={this.state.login.password ?? ''}
            onFocus={e => e.target.select()}
            placeholder="Senha"
            onChange={e => this.changeLoginPassword(e.target.value)} 
            onKeyPress={e => this.handleKeyPressPassword(e)}
            ref={this.pwdRef} />
          {this.state.login.errors?.Password ? <FormFeedback>{this.state.login.errors?.Password}</FormFeedback> : null}
        </InputGroup>
        <br/>
        <Button variant="secundary" type="button" onClick={e=>this.login()}>Login</Button>
        <Button variant="secundary" type="button" onClick={e=>this.cancel()} className="float-rigth">X</Button>
      </div>
    );
  }
}
