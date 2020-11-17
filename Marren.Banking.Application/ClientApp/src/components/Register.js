

import React, { Component } from 'react';
import session from './Session'
import { Button, Container, FormFeedback, Input, InputGroup, InputGroupAddon, InputGroupText } from 'reactstrap';
import { Redirect } from 'react-router-dom';

export class Register extends Component {
  static displayName = Register.name;

  constructor(props) {
    super(props);
    this.state = {
      resirect: null,
      accountId: null,
      register: { name: '', password: '', overdraftLimit: '200', overdraftTax: '0.005', openingDate: session.getDate(), initialDeposit: '100', errors: null }
    };
  }
  changeRegisterName(value) {
    const register = this.state.register;
    register.name = value;
    this.setState({ register: register });
  }

  changeRegisterPassword(value) {
    const register = this.state.register;
    register.password = value;
    this.setState({ register: register });
  }

  changeRegisterOverdraftLimit(value) {
    const register = this.state.register;
    register.overdraftLimit = value;
    this.setState({ register: register });
  }

  changeRegisterOverdraftTax(value) {
    const register = this.state.register;
    register.overdraftTax = value;
    this.setState({ register: register });
  }

  changeRegisterInitialDeposit(value) {
    const register = this.state.register;
    register.initialDeposit = value;
    this.setState({ register: register });
  }

  changeRegisterOpeningDate(value) {
    const register = this.state.register;
    register.openingDate = value;
    this.setState({ register: register });
  }

  async register() {
    const register = this.state.register;
    register.errors = null;
    this.setState({register});
    const result = await session.createAccount(
      register.name, 
      +register.overdraftLimit, 
      +register.overdraftTax, 
      register.password, 
      register.openingDate, 
      +register.initialDeposit);
    
    console.log(result);
    if (!result.ok) {
      if (result.errors && result.errors.length) {
        const errors = {};
        result.errors.forEach(x=>{
          errors[x.id] = x.message;
        });
        register.errors = errors;
        this.setState({register});
        console.log(register);
        return;
      }
    }

    this.setState({ accountId: result.data.id});
  }

  render() {

    if (this.state.redirect) {
      return <Redirect to={this.state.redirect} />
    }
    console.log(this.state.accountId);
    if (this.state.accountId) {
      return (<div>
        <h3 className="title">Cadastrar Conta</h3>
        <div className="register title">
          <h4 className="title">Sua conta foi cadastrada com sucesso. Favor anotar seu novo número de conta:</h4>
          <h3 className="title">{this.state.accountId}</h3>
          <Button variant="secundary" type="button" onClick={e=>this.setState({ redirect: '/' })}>Entrar</Button>
        </div>
      </div>);
    }

    return (
      <Container>
        <h3 className="title">Marren</h3>
        <h4 className="title">Cadastrar Conta</h4>
        <div className="register">
          <InputGroup className="register-field">
            <InputGroupAddon addonType="prepend" className="input-title">
              <InputGroupText>Nome: </InputGroupText>
            </InputGroupAddon>
            <Input
              type="text"
              invalid={!!this.state.register.errors?.Name}
              value={this.state.register.name ?? ''}
              onFocus={e => e.target.select()}
              placeholder="Nome"
              onChange={e => this.changeRegisterName(e.target.value)} />
            {this.state.register.errors?.Name ? <FormFeedback>{this.state.register.errors?.Name}</FormFeedback> : null}
          </InputGroup>
          <InputGroup className="register-field">
            <InputGroupAddon addonType="prepend" className="input-title">
              <InputGroupText>Senha: </InputGroupText>
            </InputGroupAddon>
            <Input
              type="password"
              invalid={!!this.state.register.errors?.Password}
              value={this.state.register.password ?? ''}
              onFocus={e => e.target.select()}
              placeholder="Senha"
              onChange={e => this.changeRegisterPassword(e.target.value)} />
            {this.state.register.errors?.Password ? <FormFeedback>{this.state.register.errors?.Password}</FormFeedback> : null}
          </InputGroup>
          <InputGroup className="register-field">
            <InputGroupAddon addonType="prepend" className="input-title">
              <InputGroupText>Limite Crédito: </InputGroupText>
            </InputGroupAddon>
            <Input
              type="number"
              invalid={!!this.state.register.errors?.OverdraftLimit}
              value={this.state.register.overdraftLimit ?? ''}
              onFocus={e => e.target.select()}
              placeholder="Limite Cheque Especial"
              onChange={e => this.changeRegisterOverdraftLimit(e.target.value)} />
            {this.state.register.errors?.OverdraftLimit ? <FormFeedback>{this.state.register.errors?.OverdraftLimit}</FormFeedback> : null}
          </InputGroup>
          <InputGroup className="register-field">
            <InputGroupAddon addonType="prepend" className="input-title">
              <InputGroupText>Taxa do Crédito*: </InputGroupText>
            </InputGroupAddon>
            <Input
              type="number"
              invalid={!!this.state.register.errors?.OverdraftTax}
              value={this.state.register.overdraftTax ?? ''}
              onFocus={e => e.target.select()}
              placeholder="Taxa do Cheque Especial"
              onChange={e => this.changeRegisterOverdraftTax(e.target.value)} />

            {this.state.register.errors?.OverdraftTax ? <FormFeedback>{this.state.register.errors?.OverdraftTax}</FormFeedback> : null}
          </InputGroup>
          <InputGroup className="register-field">
            <InputGroupAddon addonType="prepend" className="input-title">
              <InputGroupText>Saldo inicial: </InputGroupText>
            </InputGroupAddon>
            <Input
              type="number"
              invalid={!!this.state.register.errors?.InitialBalance}
              value={this.state.register.initialDeposit ?? ''}
              onFocus={e => e.target.select()}
              placeholder="Saldo inicial"
              onChange={e => this.changeRegisterInitialDeposit(e.target.value)} />

            {this.state.register.errors?.InitialBalance ? <FormFeedback>{this.state.register.errors?.InitialBalance}</FormFeedback> : null}
          </InputGroup>
          <InputGroup className="register-field">
            <InputGroupAddon addonType="prepend" className="input-title">
              <InputGroupText>Data Abertura**: </InputGroupText>
            </InputGroupAddon>
            <Input
              type="text"
              invalid={!!this.state.register.errors?.OpeningDate}
              value={this.state.register.openingDate ?? ''}
              onFocus={e => e.target.select()}
              placeholder="Data de Abertura"
              onChange={e => this.changeRegisterOpeningDate(e.target.value)} />
            {this.state.register.errors?.OpeningDate ? <FormFeedback>{this.state.register.errors?.OpeningDate}</FormFeedback> : null}
          </InputGroup>
          <small>* A Taxa do Crédito é cobrada todos os dias úteis do limite utilizado<br/></small>          
          <small>** A data de abertura retroativa serve para calcular os juros/taxas do período.<br/></small>
          <br/>
          <Button variant="secundary" type="button" onClick={e=>this.register()}>Abrir Conta</Button>
        </div>
      </Container>
    );
  }
}
