import React, { Component } from 'react';
import { Button, FormFeedback, Input, InputGroup, InputGroupAddon, InputGroupText } from 'reactstrap';
import session from './Session';

export class Withdraw extends Component {
  static displayName = Withdraw.name;

  constructor(props) {
    super(props);
    this.state = {
      password: '',
      cancelado: false,
      passwordOk: false,
      passwordPreenchido: false,
      balance: 0,
      message: null,
      loading: false,
      trigger: false,
    };
  }

  changeLoginPassword(password) {
    this.setState({ password, passwordOk: password && password.length });
  }

  cancel() {
    this.setState({cancelado: true, logado: false, loading: false, trigger: true}, () => {
      this.props.triggerNextStep({ trigger: 'account-options' });
    });
  }

  async login() {
    this.setState({ passwordOk: true, loading: true, passwordPreenchido: true });

    let balance;
    
    if (this.props.action == 'transfer') {
        const valueTransfer = +this.props.steps['account-transfer-value'].value.trim().replace(',','.');
        const accountIdDeposit = +this.props.steps['account-transfer-deposit-account-value'].value.trim();
        balance = await session.transfer(valueTransfer, this.state.password, accountIdDeposit);
    } else {
        const valueWithdraw = +this.props.steps['account-withdraw-value'].value.trim().replace(',','.');
        balance = await session.withdraw(valueWithdraw, this.state.password);
    }

    if (balance.ok) {
      this.setState({ balance: balance.data, loading: false, error: null }, () => {
        this.props.triggerNextStep();
      });
    } else {

      let error = balance.message;
      console.log(balance);
      if (balance.errors && balance.errors.length) {
        error = balance.errors[0].message;
      }

      this.setState({ balance: 0, loading: false, error }, () => {
        this.props.triggerNextStep({ trigger: 'account-options' });
      });
    }
  }

  render() {

    if (this.state.loading) {
      return (<div>Carregando...</div>);
    }

    if (this.state.cancelado) {
      return (<div>Saque cancelado.</div>)   
    }

    if (this.state.error) {
      return (<div>{this.state.error}</div>);
    }

    if (!this.state.passwordPreenchido) {
      return (<div>
        {this.props.action != 'transfer' ? 
          <div>Confirme seu saque digitando a senha da sua conta: </div> :
          <div>Confirme sua transferência digitando a senha da sua conta: </div> }
        <InputGroup>
          <Input
            type="password"
            invalid={!!this.state.error}
            value={this.state.password ?? ''}
            onFocus={e => e.target.select()}
            placeholder="Senha"
            onChange={e => this.changeLoginPassword(e.target.value)} />
          {this.state.error ? <FormFeedback>{this.state.error}</FormFeedback> : null}
        </InputGroup>
        <br/>
        <Button variant="secundary" type="button" disabled={!this.state.passwordOk} onClick={e => this.login()}>Confirmar</Button>
        <Button variant="secundary" type="button" onClick={e=>this.cancel()} className="float-rigth">X</Button>
      </div>);
    }

    return (
      <div>
        {this.props.action != 'transfer' ? 
          <div>Saque realizado.</div> : 
          <div>Transferência realizada.</div> } 
        Agora seu saldo atual é {this.state.balance.toFixed(2).toString().replace('.', ',')}.
        {this.state.balance < 0 ? <div>Você está usando o cheque especial. Fale com seu gerente sobre melhores opções de financiamento.</div> : null}
      </div>
    );
  }
}
